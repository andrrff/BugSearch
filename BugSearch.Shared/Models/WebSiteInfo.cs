using BugSearch.Shared.Interfaces;
using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;

public class WebSiteInfo : IWebSiteInfo
{
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("favicon")]
    public string? Favicon { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public double Pts { get; set; } = 0;
}