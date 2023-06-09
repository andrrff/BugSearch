using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using BugSearch.Shared.Models;
using BugSearch.Shared.Extensions;
using Microsoft.Extensions.Configuration;

namespace BugSearch.Shared.Services;

public class DatabaseConntection
{
    private readonly IMongoCollection<EventCrawler> _collectionEventCrawler;
    private readonly IMongoCollection<Dictionary> _collectionDictionary;

    public DatabaseConntection()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var url      = Environment.GetEnvironmentVariable("MONGO_DATABASE_URL") ?? configuration.GetSection("Mongo:DatabaseUrl").Value;
        var username = Environment.GetEnvironmentVariable("MONGO_USERNAME") ?? configuration.GetSection("Mongo:Username").Value;
        var password = Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? configuration.GetSection("Mongo:Password").Value;

        var databaseName               = Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? configuration.GetSection("Mongo:Database").Value;
        var collectionDictionaryName   = Environment.GetEnvironmentVariable("MONGO_COLLECTION_DICTIONARY") ?? configuration.GetSection("Mongo:Collections:Dictionary").Value;
        var collectionEventCrawlerName = Environment.GetEnvironmentVariable("MONGO_COLLECTION_EVENT_CRAWLER") ?? configuration.GetSection("Mongo:Collections:EventCrawler").Value;

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

    public string SugestedQueryByLevenshtein(string query)
    {
        var terms = _collectionDictionary.Find(x => true).ToList();
        var queryTerms = query.Split(' ');

        StringBuilder sugestedQuery = new();

        foreach (var term in queryTerms)
        {
            var sugestedTerm = terms
                .Where(x => x.Term.LevenshteinDistance(term) < 3)
                .OrderBy(x => x.Term.LevenshteinDistance(term))
                .FirstOrDefault();

            if (sugestedTerm is not null)
            {
                sugestedQuery.Append($"{sugestedTerm.Term} ");
            }
            else
            {
                sugestedQuery.Append($"{term} ");
            }
        }

        return sugestedQuery.ToString().Trim();
    }

    public SearchResult FindWebSites(string id, string query, int currentPage, int itemsPerPage = 20)
    {
        SearchResult result   = new(id, query);
        HashSet<string> terms = new(StringComparer.OrdinalIgnoreCase);

        query = query.NormalizeComplex();
        foreach (var term in query.Split(' '))
        {
            var normalizedTerm = term.NormalizeComplex();
            if (query.Length > 3 ? normalizedTerm.Length > 2 : normalizedTerm.Length > 1)
            {
                terms.Add(normalizedTerm);
            }
        }

        List<EventCrawler> collectionEvents = new();

        Parallel.ForEach(terms, term =>
        {
            var termNormalized = term.NormalizeComplex();
            var filter = Builders<EventCrawler>.Filter.Or(
                Builders<EventCrawler>.Filter.Regex(x => x.Terms, new BsonRegularExpression(termNormalized, "il")),
                Builders<EventCrawler>.Filter.Regex(x => x.Title, new BsonRegularExpression(termNormalized, "il")),
                Builders<EventCrawler>.Filter.Regex(x => x.Url, new BsonRegularExpression(termNormalized, "il")),
                Builders<EventCrawler>.Filter.Regex(x => x.Description, new BsonRegularExpression(termNormalized, "il")),
                Builders<EventCrawler>.Filter.Regex(x => x.Name, new BsonRegularExpression(termNormalized, "il"))
            );

            collectionEvents.AddRange(_collectionEventCrawler.Find(filter).ToList());
        });

        var distinctEvents = collectionEvents.DistinctBy(x => x.Title).ToList();

        Parallel.ForEach(distinctEvents, x =>
        {
            Parallel.ForEach(terms, term => 
            {
                x.Pts += (
                    (x.Body.ContainsNormalized(term) ? 1 * term.Length * 10 : term.Length * -30) +
                    (x.Body.ContainsNormalized(query) ? 1 * term.Length * 50 : term.Length * -1) +
                    (x.Description.ContainsNormalized(term) ? 1 * term.Length * 40 : term.Length * -8) +
                    (x.Description.ContainsNormalized(query) ? 1 * term.Length * 50 : term.Length * -10) +
                    (x.Url.ContainsNormalized(term) ? 1 * term.Length * 25 : term.Length * -5) +
                    (x.Url.ContainsNormalized(query) ? 1 * term.Length * 30 : term.Length * -2.5) +
                    (x.Title.ContainsNormalized(term) ? 1 * term.Length * 60 : term.Length * -10) +
                    (x.Title.ContainsNormalized(query) ? 1 * term.Length * 70 : term.Length * -5) +
                    (x.Name.ContainsNormalized(term) ? 1 * term.Length * 20 : term.Length * -0.1) +
                    (x.Name.ContainsNormalized(query) ? 1 * term.Length * 30 : term.Length * -0.3) +
                    (x.Title.LevenshteinDistance(term) * term.Length * -0.31) +
                    (x.Name.LevenshteinDistance(term) * term.Length * -0.15));
            });

            x.Pts += (
                x.Body.CalculateMeanWordDistance(terms.ToArray()) * -3.25 +
                x.Description.CalculateMeanWordDistance(terms.ToArray()) * -4.3 +
                x.Url.CalculateMeanWordDistance(terms.ToArray()) * -1.1 +
                x.Title.CalculateMeanWordDistance(terms.ToArray()) * -5.4 +
                x.Name.CalculateTFIDFLog(terms.ToArray()) * 0.5 +
                x.Title.CalculateTFIDFLog(terms.ToArray()) * 0.5 +
                x.Description.CalculateTFIDFLog(terms.ToArray()) * 0.5 +
                x.Url.CalculateTFIDFLog(terms.ToArray()) * 0.5 +
                x.Body.CalculateTFIDFLog(terms.ToArray()) * 0.5
            );

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
}