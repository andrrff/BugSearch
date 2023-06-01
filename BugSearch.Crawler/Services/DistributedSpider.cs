using DotnetSpider;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class DistributedSpider
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var builder = Builder.CreateBuilder<RobotSpider>(options =>
        {
            options.Speed = 100;
        });
        // var configuration = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
        //     .Build();

        // builder.UseRabbitMQ(new Action<RabbitMQOptions> (options =>
        // {
        //     options.HostName = configuration["Values:RABBITMQ_HOSTNAME"];
        //     options.Port     = int.Parse(configuration["Values:RABBITMQ_PORT"] ?? "0");
        //     options.UserName = configuration["Values:RABBITMQ_USERNAME"];
        //     options.Password = configuration["Values:RABBITMQ_PASSWORD"];
        //     options.Exchange = configuration["Values:RABBITMQ_EXCHANGE"];
        // }));

        builder.UseRabbitMQ(new Action<RabbitMQOptions> (options =>
        {
            options.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
            options.Port     = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "0");
            options.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
            options.Exchange = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE");
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync(cancellationToken);
    }
}