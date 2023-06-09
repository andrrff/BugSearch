using Serilog;
using DotnetSpider.DataFlow;

namespace BugSearch.Crawler.Services;

public class ConsoleLogging : DataFlowBase
{
    public static IDataFlow CreateFromOptions(IConfiguration configuration)
    {
        return new ConsoleLogging();
    }

    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public override Task HandleAsync(DataFlowContext context)
    {
        if (IsNullOrEmpty(context))
        {
            Log.Warning("DataFlowContext does not contain parsing results");
            return Task.CompletedTask;
        }

        var data = context.GetData();

        return Task.CompletedTask;
    }
}