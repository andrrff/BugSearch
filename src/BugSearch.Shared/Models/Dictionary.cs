using MongoDB.Bson;
using BugSearch.Shared.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace BugSearch.Shared.Models;

public class Dictionary : IDictionary
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id { get; set; } 
    
    public string? Term { get; set; }
}