using MongoDB.Bson;
using BugSearch.Shared.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace BugSearch.Shared.Models;

public class EventCrawler : IWebSiteInfo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id { get; set; } 
    
    public string? Name { get; set; }
    
    public string? Url { get; set; }

    public string? Favicon { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }
    
    public string? Type { get; set; }
    
    public string? Image { get; set; }
    
    public string? Locale { get; set; }

    public string? Body { get; set; }

    public string[] Terms { get; set; } = Array.Empty<string>();

    public double Pts { get; set; } = 0;
}