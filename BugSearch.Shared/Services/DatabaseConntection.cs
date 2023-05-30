using MongoDB.Driver;
using BugSearch.Shared.Models;
using System.Text.RegularExpressions;

namespace BugSearch.Shared.Services;

public class DatabaseConntection
{
    private readonly IMongoCollection<EventCrawler> _collectionEventCrawler;
    private readonly IMongoCollection<Dictionary> _collectionDictionary;

    private readonly IMongoCollection<Summary> _collectionSummary;

    public DatabaseConntection()
    {
        const string connectionUri = "mongodb://adminuser:password123@mongodb-service.crawler-bot.svc.cluster.local:27017/admin";

        var client   = new MongoClient(connectionUri);
        var database = client.GetDatabase("BugSearchDBV2");

        if (!database.ListCollectionNames().ToList().Contains("eventcrawler"))
        {
            database.CreateCollection("eventcrawler");
        }

        if (!database.ListCollectionNames().ToList().Contains("dictionary"))
        {
            database.CreateCollection("dictionary");
        }

        _collectionEventCrawler = database.GetCollection<EventCrawler>("eventcrawler");
        _collectionDictionary   = database.GetCollection<Dictionary>("dictionary");
        _collectionSummary      = database.GetCollection<Summary>("summary");
    }

    public void InsertEventCrawler(EventCrawler eventCrawler)
    {
        _collectionEventCrawler.InsertOne(eventCrawler);

        _collectionEventCrawler.ReplaceOne(
            Builders<EventCrawler>.Filter.Eq(x => x.Url, eventCrawler.Url),
            eventCrawler,
            new ReplaceOptions { IsUpsert = true }
        );

        if (eventCrawler.Terms is null)
        {
            return;
        }

        eventCrawler.Terms.ToList().ForEach(term =>
        {
            _collectionDictionary.UpdateOne(
                Builders<Dictionary>.Filter.Eq(x => x.Term, term),
                Builders<Dictionary>.Update.Set(x => x.Term, term),
                new UpdateOptions { IsUpsert = true }
            );
        });
    }

    public SummaryResult GetSummary()
    {
        var result = new SummaryResult();

        result.Summary = new Summary();

        result.Summary.IndexedPages = _collectionEventCrawler.CountDocuments(x => true);
        result.Summary.IndexedTerms = _collectionDictionary.CountDocuments(x => true);

        return result;
    }

    public SearchResult FindWebSites(string query, int limit)
    {
        SearchResult result = new();
        List<string> terms = new();

        terms = Regex
            .Unescape(Regex.Replace(query, @"\s+", " ").Trim())
            .Split(" ")
            .Distinct()
            .ToList();

        Parallel.ForEach(terms, term => 
        {
            var collectionEvents = _collectionEventCrawler.Find(x => x.Terms.Contains(term.ToLower()) ||
                                                                         (string.IsNullOrEmpty(x.Title) ||
                                                                         x.Title.Contains(term)) ||
                                                                         (string.IsNullOrEmpty(x.Url) ||
                                                                         x.Url.Contains(term)))
                                                              .Limit(limit)
                                                              .ToList();

            Parallel.ForEach(collectionEvents, x =>
            {
                if (string.IsNullOrEmpty(x.Description))
                {
                    x.Body ??= string.Empty;

                    var index = x.Body.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                    var start = index - 150;
                    var end = index + 150;

                    if (start < 0)
                    {
                        start = 0;
                    }

                    if (end > x.Body.Length)
                    {
                        end = x.Body.Length;
                    }

                    x.Description = x.Body[start..end];
                }

                x.Pts = x.Terms.Count(term.ToLower().Contains) +
                        (string.IsNullOrEmpty(x.Title) ? 0 : (x.Title.Contains(term) ? 1 * 10 : 0)) +
                        (string.IsNullOrEmpty(x.Url) ? 0 : (x.Url.Contains(term) ? 1 * 7 : 0));

                result.SearchResults.Add(new WebSiteInfo
                {
                    Link        = x.Url,
                    Favicon     = x.Favicon,
                    Title       = x.Title,
                    Description = x.Description,
                    Pts         = x.Pts
                });
            });
        });

        result.SearchResults = result.SearchResults.OrderByDescending(x => x.Pts).ToList();

        return result;
    }
}