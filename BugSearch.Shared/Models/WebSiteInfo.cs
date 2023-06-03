using BugSearch.Shared.Interfaces;
using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;

public class WebSiteInfo : IWebSiteInfo
{
    [JsonPropertyName("link")]
    public string? Link { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("favicon")]
    public string? Favicon { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    public double Pts { get; set; } = 0;
}