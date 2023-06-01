using k8s;
using System.Text;
using System.Text.Json;

namespace BugSearch.Shared.Services;

public class KubernetesClient
{
    public static string GetSecret(string name, string data, string ns = "crawler-bot")
    {
        var config = KubernetesClientConfiguration.InClusterConfig();
        var client = new Kubernetes(config);
        var secret = client.ReadNamespacedSecret(name, ns);

        return Encoding.UTF8.GetString(secret.Data[data]);
    }

    public static string GetConfigMap(string name, string data, string ns = "crawler-bot")
    {
        var config    = KubernetesClientConfiguration.InClusterConfig();
        var client    = new Kubernetes(config);
        var configMap = client.ReadNamespacedConfigMap(name, ns);

        return configMap.Data[data];
    }

    public static string GetAllSecrets(string ns = "crawler-bot")
    {
        var config = KubernetesClientConfiguration.InClusterConfig();
        var client = new Kubernetes(config);
        var secret = client.ListSecretForAllNamespaces();

        return JsonSerializer.Serialize(secret);
    }
}