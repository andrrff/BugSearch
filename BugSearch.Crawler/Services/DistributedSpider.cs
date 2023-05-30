using DotnetSpider;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class DistributedSpider
{
    public static async Task RunAsync()
    {

        var builder = Builder.CreateBuilder<RobotSpider>(options =>
        {
            options.Speed = 2;
        });

        builder.UseRabbitMQ(new Action<RabbitMQOptions> (options =>
        {
            options.HostName = "20.124.78.150";
            options.Port     = 5672;
            options.UserName = "guest";
            options.Password = "guest";
            options.Exchange = "BugSearch";
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }
}