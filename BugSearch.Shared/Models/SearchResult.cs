using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;
 
public class SearchResult
{
    [JsonPropertyName("searchResults")]
    public List<WebSiteInfo> SearchResults { get; set; } = new List<WebSiteInfo>();
}