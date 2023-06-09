using System.Text.Json.Serialization;

namespace  BugSearch.Shared.Models;

public class CrawlerProperties
{
    [JsonPropertyName("useMessageQueue")]
    public bool UseMessageQueue { get; set; }

    [JsonPropertyName("persistData")]
    public bool PersistData { get; set; }

    [JsonPropertyName("speed")]
    public int Speed { get; set; }

    [JsonPropertyName("depth")]
    public int Depth { get; set; }

    public CrawlerProperties(bool useMessageQueue, bool persistData, int speed, int depth)
    {
        UseMessageQueue = useMessageQueue;
        PersistData     = persistData;
        Speed           = speed;
        Depth           = depth;
    }
}