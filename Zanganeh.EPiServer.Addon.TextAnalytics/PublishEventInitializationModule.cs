﻿using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html;
using EPiServer.Framework;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Initialization = EPiServer.Framework.Initialization;
using Web = EPiServer.Web;

namespace Zanganeh.EPiServer.Addon.TextAnalytics
{
    [InitializableModule]
    [ModuleDependency(typeof(Web.InitializationModule))]
    public class PublishEventInitializationModule : IInitializableModule
    {
        public void Initialize(Initialization.InitializationEngine context)
        {
            contentEvents.Service.PublishingContent += PublishingContent;
        }

        void PublishingContent(object sender, ContentEventArgs e)
        {
            var productPage = e.Content as ITageablePage;

            if (productPage != null)
            {
                var textAnalysisContentValues = GetTextAnalysisRequireentPropertyValues(productPage);

                productPage.Tags = string.Join(",", textAnalyticsService.Service.KeyPhrases("en", textAnalysisContentValues));
            }
        }

        public void Uninitialize(Initialization.InitializationEngine context)
        {
            contentEvents.Service.PublishingContent -= PublishingContent;
        }

        static IEnumerable<string> GetTextAnalysisRequireentPropertyValues(ITageablePage productPage)
        {
            var textAnalysisContentProperties = productPage
                .GetType()
                .BaseType
                .GetProperties();

            foreach (var textAnalysisContentProperty in textAnalysisContentProperties)
            {
                if (textAnalysisContentProperty.GetCustomAttributes(typeof(TextAnalysisRequireAttribute), true).Any())
                {
                    var propertyVale = textAnalysisContentProperty.GetValue(productPage);
                    if (propertyVale != null)
                    {
                        var strPropertyValue = propertyVale.ToString();
                        if (!string.IsNullOrWhiteSpace(strPropertyValue))
                        {
                            var noHtmlString = HttpUtility.HtmlDecode(TextIndexer.StripHtml(strPropertyValue, 0));
                            yield return Regex.Replace(noHtmlString, @"[^\u0000-\u007F]+", string.Empty);
                        }
                    }
                }
            }
        }

        Injected<IContentEvents> contentEvents;
        Injected<ITextAnalyticsService> textAnalyticsService;
    }
}