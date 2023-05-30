using System.Text.Json.Serialization;

namespace BugSearch.Api.Models;

public class Message
{
    [JsonPropertyName("role")]
    public string? role { get; set; }

    [JsonPropertyName("content")]
    public string? content { get; set; }
}

public class OpenAIPromptRequest
{
    [JsonPropertyName("model")]
    public string? model { get; set; }

    [JsonPropertyName("messages")]
    public List<Message>? messages { get; set; }

    [JsonPropertyName("temperature")]
    public double? temperature { get; set; }
}
