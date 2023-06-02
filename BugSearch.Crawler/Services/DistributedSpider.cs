using DotnetSpider;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class DistributedSpider
{
    public static async Task RunAsync(CancellationToken cancellationToken = default, int speed = 100, int depth = 0)
    {
        var builder = Builder.CreateBuilder<RobotSpider>(options =>
        {
            options.Speed = 100;
            options.Batch = 100;
            
            if (depth > 0) options.Depth = depth;
        });

        builder.UseRabbitMQ(new Action<RabbitMQOptions>(options =>
        {
            options.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
            options.Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "0");
            options.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
            options.Exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE");
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync(cancellationToken);
    }
}