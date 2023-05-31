using k8s;
using System.Text;

namespace BugSearch.Shared.Services;

public class KubernetesClient
{
    public static string GetKubernetesSecret(string name, string data, string ns = "crawler-bot")
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        var client = new Kubernetes(config);
        var secret = client.ReadNamespacedSecret(name, ns);

        return Encoding.UTF8.GetString(secret.Data[data]);
    }
}