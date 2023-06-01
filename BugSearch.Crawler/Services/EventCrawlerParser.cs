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
            var url         =  context.Request.RequestUri;
            var title       =  context.Selectable.XPath(".//title")?.Value;
            var favicon     =  context.Selectable.XPath(".//link[@rel='icon']")?.Value;
            var description =  context.Selectable.XPath(".//meta[@name='description']")?.Value ?? string.Empty;
            var body        =  Regex.Replace(Regex.Replace(Regex.Replace(Regex.Unescape(Regex.Replace(Regex.Replace(context.Selectable.XPath(".//body").Value, @"\s+", " ").Trim(), @"\W+", "")), @"(\B[A-Z])", " $1"), @"([a-z])([A-Z])", "$1 $2"), $@"\b\w{{1,{2 - 1}}}\b", "");
            var terms       =  body.Split(" ").Distinct().Select(term => term.ToLower()).ToArray();

            if (!string.IsNullOrEmpty(title) || string.IsNullOrEmpty(body))
            {
                context.AddData(typeName, new EventCrawler
                {
                    Url         = url.ToString(),
                    Title       = title,
                    Favicon     = favicon,
                    Description = description,
                    Body        = body,
                    Terms       = terms
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