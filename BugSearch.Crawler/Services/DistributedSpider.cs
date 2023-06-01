using DotnetSpider;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using BugSearch.Shared.Services;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class DistributedSpider
{
    public static async Task RunAsync()
    {

        var builder = Builder.CreateBuilder<RobotSpider>(options =>
        {
            options.Speed = 100;
        });

        builder.UseRabbitMQ(new Action<RabbitMQOptions> (options =>
        {
            options.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
            options.Port     = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "0");
            options.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
            options.Exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE");
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }
}