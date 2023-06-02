using MongoDB.Driver;
using BugSearch.Shared.Models;
using System.Text.RegularExpressions;

namespace BugSearch.Shared.Services;

public class DatabaseConntection
{
    private readonly IMongoCollection<EventCrawler> _collectionEventCrawler;
    private readonly IMongoCollection<Dictionary> _collectionDictionary;

    public DatabaseConntection()
    {
        var url      = Environment.GetEnvironmentVariable("MONGO_DATABASE_URL") ?? "localhost:27017";
        var username = Environment.GetEnvironmentVariable("MONGO_USERNAME") ?? "admin";
        var password = Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? "senha_admin";

        var databaseName               = Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? "BugSearchDBV2";
        var collectionDictionaryName   = Environment.GetEnvironmentVariable("MONGO_COLLECTION_DICTIONARY") ?? "dictionary";
        var collectionEventCrawlerName = Environment.GetEnvironmentVariable("MONGO_COLLECTION_EVENT_CRAWLER") ?? "eventcrawler";

        var connectionUri = $"mongodb://{username}:{password}@{url}/admin";

        var client   = new MongoClient(connectionUri);
        var database = client.GetDatabase(databaseName);

        if (!database.ListCollectionNames().ToList().Contains(collectionEventCrawlerName))
        {
            database.CreateCollection(collectionEventCrawlerName);
        }

        if (!database.ListCollectionNames().ToList().Contains(collectionDictionaryName))
        {
            database.CreateCollection(collectionDictionaryName);
        }

        _collectionEventCrawler = database.GetCollection<EventCrawler>(collectionEventCrawlerName);
        _collectionDictionary   = database.GetCollection<Dictionary>(collectionDictionaryName);
    }

    public void InsertEventCrawler(EventCrawler eventCrawler)
    {
        _collectionEventCrawler.FindOneAndDelete(x => x.Url == eventCrawler.Url);
        _collectionEventCrawler.InsertOne(eventCrawler);

        Parallel.ForEach(eventCrawler.Terms.ToList(), term => 
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
        query = query.ToLower();

        terms = Regex
            .Unescape(Regex.Replace(query, @"\s+", " ").Trim())
            .Split(" ")
            .Distinct()
            .Where(term => query.Length > 3 ? term.Length > 2 : term.Length > 1)
            .ToList();

        Parallel.ForEach(terms, term => 
        {
            term = term.ToLower();

            var collectionEvents = _collectionEventCrawler.Find(x => x.Terms.Contains(term.ToLower()) ||
                                                                         (!string.IsNullOrEmpty(x.Title) &&
                                                                         x.Title.ToLower().Contains(term)) ||
                                                                         (!string.IsNullOrEmpty(x.Url) &&
                                                                         x.Url.ToLower().Contains(term)) ||
                                                                         (!string.IsNullOrEmpty(x.Description) &&
                                                                         x.Description.ToLower().Contains(term)))
                                                              .Limit(limit)
                                                              .ToList();

            Parallel.ForEach(collectionEvents, x =>
            {
                x.Pts = (string.IsNullOrEmpty(x.Title) ? 0 : (x.Title.ToLower().Contains(term)  ? 1 * term.Length * 30 : term.Length * -10)) +
                        (string.IsNullOrEmpty(x.Title) ? 0 : (x.Title.ToLower().Contains(query) ? 1 * term.Length * 50 : term.Length * -5)) +
                        (string.IsNullOrEmpty(x.Url)   ? 0 : (x.Url.ToLower().Contains(term)    ? 1 * term.Length * 15 : term.Length * -5)) +
                        (string.IsNullOrEmpty(x.Url)   ? 0 : (x.Url.ToLower().Contains(query)   ? 1 * term.Length * 30 : term.Length * -2.5)) +
                        (string.IsNullOrEmpty(x.Description) ? 0 : (x.Description.ToLower().Contains(term) ? 1 * term.Length * 40 : term.Length * -8)) +
                        (string.IsNullOrEmpty(x.Description) ? 0 : (x.Description.ToLower().Contains(query) ? 1 * term.Length * 50 : term.Length * -10)) +
                        (string.IsNullOrEmpty(x.Body)  ? 0 : (x.Body.ToLower().Contains(term)   ? 1 * term.Length *  5 : term.Length * -30)) +
                        (string.IsNullOrEmpty(x.Body)  ? 0 : (x.Body.ToLower().Contains(query)  ? 1 * term.Length * 50 : term.Length * -1));

                if (string.IsNullOrEmpty(x.Description))
                {
                    x.Body ??= string.Empty;

                    var index = x.Body.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                    var start = index - 150;
                    var end   = index + 150;

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

        result.SearchResults = result.SearchResults.OrderByDescending(x => x.Pts).DistinctBy(x => x.Title).ToList();

        return result;
    }
}