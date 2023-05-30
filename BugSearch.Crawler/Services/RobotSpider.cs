using DotnetSpider;
using DotnetSpider.Http;
using DotnetSpider.DataFlow;
using DotnetSpider.Scheduler;
using DotnetSpider.Downloader;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Options;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.RabbitMQ;

namespace BugSearch.Crawler.Services;

public class RobotSpider : Spider
{
    public RobotSpider(IOptions<SpiderOptions> options, DependenceServices services,
        ILogger<Spider> logger) : base(
        options, services, logger)
    {
    }

    public static async Task RunAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var rabbitMQOptions = new RabbitMQOptions();
        
        configuration.GetSection("RabbitMQ").Bind(rabbitMQOptions);

        var builder = Builder.CreateDefaultBuilder<RobotSpider>(x =>
        {
            x.Speed = 100;
        });

        builder.UseRabbitMQ(new Action<RabbitMQOptions> (x => x = rabbitMQOptions));
        builder.IgnoreServerCertificateError();
        builder.UseDownloader<HttpClientDownloader>();
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();

        await builder.Build().RunAsync();
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
                    Timeout = 10000
                }));
        }, stoppingToken);

        AddDataFlow(new EventCrawlerParser());
        AddDataFlow(new Persistence());
        AddDataFlow(new ConsoleStorage());
    }

    protected override SpiderId GenerateSpiderId()
    {
        return new(ObjectId.CreateId().ToString(), GetType().FullName);
    }
}