using MongoDB.Driver;
using BugSearch.Shared.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

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

    public SearchResult FindWebSites(string id, string query, int currentPage, int itemsPerPage = 20)
    {
        SearchResult result = new(id, query);
        List<string> terms = new();
        query = NormalizeString(query);

        terms = Regex
            .Unescape(Regex.Replace(query, @"\s+", " ").Trim())
            .Split(" ")
            .Distinct()
            .Where(term => query.Length > 3 ? term.Length > 2 : term.Length > 1)
            .ToList();

        var collectionEvents = new List<EventCrawler>();

        Parallel.ForEach(terms, term => 
        {
            term = NormalizeString(term);

            collectionEvents.AddRange(_collectionEventCrawler.Find(x => true)
                                                             .ToList()
                                                             .FindAll(x => x.Terms.Contains(term.ToLower()) ||
                                                                    (!string.IsNullOrEmpty(x.Title) && NormalizeString(x.Title).Contains(NormalizeString(term))) ||
                                                                    (!string.IsNullOrEmpty(x.Url) && NormalizeString(x.Url).Contains(NormalizeString(term))) ||
                                                                    (!string.IsNullOrEmpty(x.Description) && NormalizeString(x.Description).Contains(NormalizeString(term))) ||
                                                                    (!string.IsNullOrEmpty(x.Name) && NormalizeString(x.Name).Contains(NormalizeString(term))))
                                                             .ToList());
        });

        collectionEvents = collectionEvents.DistinctBy(x => x.Title).ToList();

        Parallel.ForEach(collectionEvents, x =>
        {
            Parallel.ForEach(terms, term => 
            {
                x.Pts += (
                        (string.IsNullOrEmpty(x.Name) ? 0 : (NormalizeString(x.Name).Contains(term) ? 1 * term.Length * 20 : term.Length * -0.1)) +
                        (string.IsNullOrEmpty(x.Name) ? 0 : (NormalizeString(x.Name).Contains(query) ? 1 * term.Length * 30 : term.Length * -0.3)) +
                        (string.IsNullOrEmpty(x.Description) ? 0 : (NormalizeString(x.Description).Contains(term) ? 1 * term.Length * 40 : term.Length * -8)) +
                        (string.IsNullOrEmpty(x.Description) ? 0 : (NormalizeString(x.Description).Contains(query) ? 1 * term.Length * 50 : term.Length * -10)) +
                        (string.IsNullOrEmpty(x.Url) ? 0 : (NormalizeString(x.Url).Contains(term) ? 1 * term.Length * 25 : term.Length * -5)) +
                        (string.IsNullOrEmpty(x.Url) ? 0 : (NormalizeString(x.Url).Contains(query) ? 1 * term.Length * 30 : term.Length * -2.5)) +
                        (string.IsNullOrEmpty(x.Title) ? 0 : (NormalizeString(x.Title).Contains(term) ? 1 * term.Length * 60 : term.Length * -10)) +
                        (string.IsNullOrEmpty(x.Title) ? 0 : (NormalizeString(x.Title).Contains(query) ? 1 * term.Length * 70 : term.Length * -5)) +
                        (string.IsNullOrEmpty(x.Body) ? 0 : (NormalizeString(x.Body).Contains(term) ? 1 * term.Length * 10 : term.Length * -30)) +
                        (string.IsNullOrEmpty(x.Body) ? 0 : (NormalizeString(x.Body).Contains(query) ? 1 * term.Length * 50 : term.Length * -1)));

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
            });

            result.SearchResults.Add(new WebSiteInfo
            {
                Name        = x.Name,
                Link        = x.Url,
                Favicon     = x.Favicon,
                Title       = x.Title,
                Description = x.Description,
                Type        = x.Type,
                Image       = x.Image,
                Locale      = x.Locale,
                Pts         = x.Pts
            });
        });

        result.SearchResults = result.SearchResults.OrderByDescending(x => x.Pts).ToList();

        return result;
    }

    private string NormalizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string normalizedString = input.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }


}