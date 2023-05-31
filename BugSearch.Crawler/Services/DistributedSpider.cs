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
            options.HostName = EnvironmentService.GetValue("RABBITMQ_HOSTNAME");
            options.Port     = int.Parse(EnvironmentService.GetValue("RABBITMQ_PORT"));
            options.UserName = EnvironmentService.GetValue("RABBITMQ_USERNAME");
            options.Password = EnvironmentService.GetValue("RABBITMQ_PASSWORD");
            options.Exchange = EnvironmentService.GetValue("RABBITMQ_EXCHANGE");
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }
}