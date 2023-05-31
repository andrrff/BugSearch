using k8s;
using System.Text;

namespace BugSearch.Shared.Services;

public class KubernetesClient
{
    public static string GetSecret(string name, string data, string ns = "crawler-bot")
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile("/var/run/secrets/kubernetes.io/serviceaccount");
        var client = new Kubernetes(config);
        var secret = client.ReadNamespacedSecret(name, ns);

        return Encoding.UTF8.GetString(secret.Data[data]);
    }

    public static string GetConfigMap(string name, string data, string ns = "crawler-bot")
    {
        var config    = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        var client    = new Kubernetes(config);
        var configMap = client.ReadNamespacedConfigMap(name, ns);

        return configMap.Data[data];
    }
}