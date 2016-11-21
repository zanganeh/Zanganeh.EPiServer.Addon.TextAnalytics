using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Configuration;

namespace Zanganeh.EPiServer.Addon.TextAnalytics
{
    public interface ITextAnalyticsService
    {
        IEnumerable<string> KeyPhrases(string language, IEnumerable<string> contents);
    }

    [ServiceConfiguration(typeof(ITextAnalyticsService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TextAnalyticsService : ITextAnalyticsService
    {
        public TextAnalyticsService()
        {
            apiConfig = WebConfigurationManager.AppSettings["AzureTextAnalyticsSubscriptionID"];
            apiUrl = WebConfigurationManager.AppSettings["AzureTextAnalyticsUrl"];
        }

        public IEnumerable<string> KeyPhrases(string language, IEnumerable<string> contents)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(ApiSubscriptionAppSettingKey, apiConfig);
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");

                var document = new Request { Documents = contents.Select((item, index) => new Content { Id = index, Language = language, Text = item }).ToArray() };
                var requestContent = JsonConvert.SerializeObject(document);

                var result = webClient.UploadString(apiUrl, requestContent);

                return JsonConvert.DeserializeObject<Response>(result).Documents.SelectMany(a => a.keyPhrases);
            }
        }

        readonly string apiConfig;
        readonly string apiUrl;
        const string ApiSubscriptionAppSettingKey = "Ocp-Apim-Subscription-Key";
    }

    class Content
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public string Text { get; set; }
    }

    class Request
    {
        public Content[] Documents { get; set; }
    }

    class KeyPhrases
    {
        public int Id { get; set; }
        public string[] keyPhrases { get; set; }
    }

    class Response
    {
        public KeyPhrases[] Documents { get; set; }
        public string[] Errors { get; set; }
    }
}
