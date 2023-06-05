using Serilog;
using DotnetSpider;
using DotnetSpider.Http;
using DotnetSpider.DataFlow;
using DotnetSpider.Scheduler;
using DotnetSpider.Downloader;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Options;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class RobotSpider : Spider
{
    public RobotSpider(IOptions<SpiderOptions> options, DependenceServices services,
        ILogger<Spider> logger) : base(
        options, services, logger)
    {
    }

    public static async Task RunAsync(CancellationToken cancellationToken = default, int speed = 100, int depth = 0)
    {
        var builder = Builder.CreateDefaultBuilder<RobotSpider>(options =>
        {
            options.Speed = speed;

            if (depth > 0) options.Depth = depth;
        });

        builder.UseSerilog();
        builder.IgnoreServerCertificateError();
        builder.UseDownloader<HttpClientDownloader>();
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();

        await builder.Build().RunAsync(cancellationToken);
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() =>
        {
            RobotSingleton
                .GetInstance()
                .GetUrls()
                .ToList()
                .ForEach(async url => await AddRequestsAsync(new Request(url)
                {
                    Timeout = 1000000
                }));
        }, stoppingToken);

        AddDataFlow(new EventCrawlerParser());

        if (RobotSingleton.GetInstance().PersistData)
            AddDataFlow(new Persistence());
            
        AddDataFlow(new ConsoleStorage());
    }

    protected override SpiderId GenerateSpiderId()
    {
        return new(ObjectId.CreateId().ToString(), GetType().FullName);
    }
}