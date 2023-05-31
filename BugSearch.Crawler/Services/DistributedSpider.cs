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
            options.HostName = KubernetesClient.GetConfigMap("rabbitmq-creds", "hostname");
            options.Port     = int.Parse(KubernetesClient.GetConfigMap("rabbitmq-creds", "port"));
            options.UserName = KubernetesClient.GetSecret("rabbitmq-creds", "username");
            options.Password = KubernetesClient.GetSecret("rabbitmq-creds", "password");
            options.Exchange = KubernetesClient.GetConfigMap("rabbitmq-creds", "exchange");
        }));
        builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
        await builder.Build().RunAsync();
    }
}