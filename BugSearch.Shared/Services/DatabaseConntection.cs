using System.Text;
using MongoDB.Driver;
using System.Globalization;
using BugSearch.Shared.Models;
using System.Collections.Concurrent;

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
        SearchResult result   = new(id, query);
        HashSet<string> terms = new(StringComparer.OrdinalIgnoreCase);

        query = NormalizeString(query);
        foreach (var term in query.Split(' '))
        {
            var normalizedTerm = NormalizeString(term);
            if (query.Length > 3 ? normalizedTerm.Length > 2 : normalizedTerm.Length > 1)
            {
                terms.Add(normalizedTerm);
            }
        }

        List<EventCrawler> collectionEvents = new();
        var collectionEventCrawler = _collectionEventCrawler.Find(x => true).ToList();

        Parallel.ForEach(terms, term =>
        {
            var normalizedTerm = NormalizeString(term);
            collectionEvents.AddRange(collectionEventCrawler
                                               .Where(x => x.Terms.Contains(normalizedTerm) ||
                                                           (!string.IsNullOrEmpty(x.Title) && NormalizeString(x.Title).Contains(normalizedTerm)) ||
                                                           (!string.IsNullOrEmpty(x.Url) && NormalizeString(x.Url).Contains(normalizedTerm)) ||
                                                           (!string.IsNullOrEmpty(x.Description) && NormalizeString(x.Description).Contains(normalizedTerm)) ||
                                                           (!string.IsNullOrEmpty(x.Name) && NormalizeString(x.Name).Contains(normalizedTerm)))
                                               .ToList());
        });

        var distinctEvents = collectionEvents.DistinctBy(x => x.Title).ToList();

        Parallel.ForEach(distinctEvents, x =>
        {
            string normalizedTitle       = NormalizeString(x.Title);
            string normalizedDescription = NormalizeString(x.Description);
            string normalizedUrl         = NormalizeString(x.Url);
            string normalizedBody        = NormalizeString(x.Body);

            foreach (var term in terms)
            {
                x.Pts += (
                    (string.IsNullOrEmpty(x.Name) ? 0 : (normalizedTitle.Contains(term) ? 1 * term.Length * 20 : term.Length * -0.1)) +
                    (string.IsNullOrEmpty(x.Name) ? 0 : (normalizedTitle.Contains(query) ? 1 * term.Length * 30 : term.Length * -0.3)) +
                    (string.IsNullOrEmpty(x.Description) ? 0 : (normalizedDescription.Contains(term) ? 1 * term.Length * 40 : term.Length * -8)) +
                    (string.IsNullOrEmpty(x.Description) ? 0 : (normalizedDescription.Contains(query) ? 1 * term.Length * 50 : term.Length * -10)) +
                    (string.IsNullOrEmpty(x.Url) ? 0 : (normalizedUrl.Contains(term) ? 1 * term.Length * 25 : term.Length * -5)) +
                    (string.IsNullOrEmpty(x.Url) ? 0 : (normalizedUrl.Contains(query) ? 1 * term.Length * 30 : term.Length * -2.5)) +
                    (string.IsNullOrEmpty(x.Title) ? 0 : (normalizedTitle.Contains(term) ? 1 * term.Length * 60 : term.Length * -10)) +
                    (string.IsNullOrEmpty(x.Title) ? 0 : (normalizedTitle.Contains(query) ? 1 * term.Length * 70 : term.Length * -5)) +
                    (string.IsNullOrEmpty(x.Body) ? 0 : (normalizedBody.Contains(term) ? 1 * term.Length * 10 : term.Length * -30)) +
                    (string.IsNullOrEmpty(x.Body) ? 0 : (normalizedBody.Contains(query) ? 1 * term.Length * 50 : term.Length * -1)));
            }

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

        result.SearchResults.Sort((x, y) => y.Pts.CompareTo(x.Pts));

        return result;
    }

    private string NormalizeString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string normalizedString = input.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }


}