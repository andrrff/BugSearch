using DotnetSpider;
using DotnetSpider.RabbitMQ;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;

namespace BugSearch.Crawler.Services;

public class DistributedSpider
{
    public static async Task RunAsync()
    {
        var RabbitMQConnection = "amqp://guest:guest@rabbitmq-service:5672/";
        new RabbitMQOptions()
        {
            
        };
        var builder = Builder.CreateBuilder<RobotSpider>(options =>
        {
            options.Speed = 2;
        });

        builder.UseRabbitMQ();
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }
}