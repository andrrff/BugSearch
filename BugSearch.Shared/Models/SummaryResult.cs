using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;

public class SummaryResult
{
    [JsonPropertyName("summary")]
    public Summary? Summary { get; set; }
}