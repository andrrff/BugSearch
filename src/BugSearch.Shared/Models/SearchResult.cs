using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;
 
public class SearchResult
{
    public SearchResult()
    {
    }

    public SearchResult(string id, string query)
    {
        Id    = id;
        Query = query;
    }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("searchResults")]
    public List<WebSiteInfo> SearchResults { get; set; } = new();

    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; set; } = new();

    public SearchResult GetPage(int currentPage, int itemsPerPage)
    {
        int totalItems = this.SearchResults.Count;
        int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
        int startIndex = (currentPage - 1) * itemsPerPage;
        int endIndex   = startIndex + itemsPerPage;

        endIndex = Math.Min(endIndex, totalItems);

        List<WebSiteInfo> currentPageItems = new(endIndex - startIndex);

        for (int i = startIndex; i < endIndex; i++)
        {
            currentPageItems.Add(this.SearchResults[i]);
        }

        return new SearchResult
        {
            Id = this.Id,
            Query = this.Query,
            SearchResults = currentPageItems,
            Pagination = new Pagination
            {
                CurrentPage  = currentPage,
                TotalPages   = totalPages,
                ItemsPerPage = itemsPerPage
            }
        };
    }
}