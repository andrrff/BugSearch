using dotenv.net;

namespace BugSearch.Shared.Services;

public class EnvironmentService {
    public EnvironmentService()
    {
        DotEnv.Load();
    }

    public static string GetValue(string key)
    {
        var envVars = DotEnv.Read();

        return envVars[key];
    }
}