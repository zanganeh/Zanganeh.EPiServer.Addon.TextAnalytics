using EPiServer.Core;

namespace Zanganeh.EPiServer.Addon.TextAnalytics
{
    public interface ITageablePage : IContent
    {
        string Tags { get; set; }
    }
}
