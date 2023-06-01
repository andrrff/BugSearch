using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;

public class CrawlerRequest
{
    [JsonPropertyName("urls")]
    public IEnumerable<string> Urls { get; set; }

    [JsonPropertyName("properties")]
    public CrawlerProperties Properties { get; set; }

    public CrawlerRequest(IEnumerable<string> urls, CrawlerProperties properties)
    {
        Urls       = urls;
        Properties = properties;
    }

}