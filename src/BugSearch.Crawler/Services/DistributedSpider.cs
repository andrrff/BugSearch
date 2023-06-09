using DotnetSpider;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class DistributedSpider
{
    public static async Task RunAsync(CancellationToken cancellationToken = default, int speed = 100, int depth = 0)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var builder = Builder.CreateBuilder<RobotSpider>(options =>
        {
            options.Speed = speed;
            
            if (depth > 0) options.Depth = depth;
        });

        builder.UseRabbitMQ(new Action<RabbitMQOptions>(options =>
        {
            options.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? configuration.GetSection("RabbitMQ:Hostname").Value;
            options.Port     = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? configuration.GetSection("RabbitMQ:Port").Value ?? "5672");
            options.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? configuration.GetSection("RabbitMQ:Username").Value;
            options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? configuration.GetSection("RabbitMQ:Password").Value;
            options.Exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? configuration.GetSection("RabbitMQ:Exchange").Value;
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync(cancellationToken);
    }
}