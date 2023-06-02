using DotnetSpider.DataFlow;
using BugSearch.Shared.Models;
using BugSearch.Shared.Services;

namespace BugSearch.Crawler.Services
{
    class Persistence : DataFlowBase
    {
        private readonly DatabaseConntection _context;

        public Persistence()
        {
            _context = new DatabaseConntection();
        }

        public override Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public override async Task HandleAsync(DataFlowContext context)
        {
            if (IsNullOrEmpty(context))
            {
                System.Console.WriteLine("DataFlowContext does not contain parsing results");
                return;
            }

            var typeName = typeof(EventCrawler).FullName;
            var data     = (EventCrawler)context.GetData(typeName);

            if (data is not null)
            {
                await Task.Run(() => _context.InsertEventCrawler(data));
            }
        }
    }
}