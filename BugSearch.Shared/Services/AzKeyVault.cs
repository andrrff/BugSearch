using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace BugSearch.Shared.Services;

public class PowerKeyVault
{
    private PowerKeyVault() { }

    private static PowerKeyVault? _instance = default!;

    private static readonly object _lock = new object();

    public SecretClient client { get; set; } = default!;

    public static PowerKeyVault GetInstance(PowerKeyVault value = default!)
    {
        if (_instance is null)
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = Construct();
                }
            }
        }

        return _instance;
    }

    private static PowerKeyVault Construct() =>
        new PowerKeyVault
        {
            client = new SecretClient(
                new Uri("https://bugsearch.vault.azure.net/"),
                new DefaultAzureCredential(),
                //new InteractiveBrowserCredential(),
                new SecretClientOptions
                {
                    Retry =
                    {
                        Delay      = TimeSpan.FromSeconds(2),
                        MaxDelay   = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode       = Azure.Core.RetryMode.Exponential
                    }
                }
            )
        };

    public async Task<KeyVaultSecret> GetKeyVaultSecretAsync(string name) => await client.GetSecretAsync(name);

    public string GetKeyVaultSecret(string name) => client.GetSecretAsync(name).Result.Value.Value.ToString();
}