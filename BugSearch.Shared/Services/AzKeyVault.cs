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
                new ClientSecretCredential(
                    "02b6749b-5ce0-4853-bd5c-a05f9bd9dd3a",
                    "7eeda667-ae25-4ed5-a187-ab146a527366",
                    "FkV8Q~6fQ9ggPSDg73djJWwZfl2LavfsZmXwWb8x"
            ))
        };

    public async Task<KeyVaultSecret> GetKeyVaultSecretAsync(string name) => await client.GetSecretAsync(name);

    public string GetKeyVaultSecret(string name) => client.GetSecretAsync(name).Result.Value.Value.ToString();
}