using DotnetSpider.Selector;
using DotnetSpider.DataFlow;
using BugSearch.Shared.Models;
using DotnetSpider.DataFlow.Parser;
using System.Text.RegularExpressions;

namespace BugSearch.Crawler.Services;

class EventCrawlerParser : DataParser
    {
        protected override Task ParseAsync(DataFlowContext context)
        {
            var typeName    = typeof(EventCrawler).FullName;
            var url         = context.Request.RequestUri;
            var title       = context.Selectable.XPath(".//title")?.Value;
            var favicon     = context.Selectable.XPath(".//link[@rel='icon']/@href")?.Value;
            var description = context.Selectable.XPath(".//meta[@name='description']/@content")?.Value;
            var body        = Regex.Replace(context.Selectable.XPath(".//body").Value ?? string.Empty, "[^a-zA-Z]+", " ") ?? string.Empty;
            var terms       = body.Split(" ", StringSplitOptions.RemoveEmptyEntries).Distinct().Select(term => term.ToLower()).Where(term => term.Length > 2).ToArray();

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(body))
            {
                context.AddData(typeName, new EventCrawler
                {
                    Url         = url.ToString(),
                    Title       = title,
                    Favicon     = favicon,
                    Description = description,
                    Body        = body,
                    Terms       = terms,
                    Pts         = (!string.IsNullOrEmpty(description) ? 10 : -10) + (!string.IsNullOrEmpty(favicon) ? 4 : 0)
                });
            }

            return Task.CompletedTask;
        }

        public override Task InitializeAsync()
        {
            AddRequiredValidator(".com");
            AddFollowRequestQuerier(Selectors.XPath("."));

            return Task.CompletedTask;
        }
    }