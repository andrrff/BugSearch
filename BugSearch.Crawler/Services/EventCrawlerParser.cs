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
            var url         = context.Selectable.XPath(".//meta[@property='og:url' or @property='twitter:url']/@content")?.Value ?? context.Request.RequestUri?.ToString() ?? string.Empty;
            var name        = context.Selectable.XPath(".//meta[@property='og:site_name' or @property='al:android:app_name' or @property='twitter:app:name:googleplay' or @property='al:iphone:app_name' or @property='al:ipad:app_name']/@content")?.Value ?? string.Empty;
            var title       = context.Selectable.XPath(".//title | .//meta[@name='title' or @property='og:title' or @property='twitter:title']/@content")?.Value ?? string.Empty;
            var favicon     = context.Selectable.XPath(".//link[@rel='icon' or @rel='shortcut icon' or @rel='apple-touch-icon']/@href")?.Value ?? string.Empty;
            var description = context.Selectable.XPath(".//meta[@name='description' or @property='twitter:description' or @property='og:description']/@content")?.Value ?? string.Empty;
            var type        = context.Selectable.XPath(".//meta[@property='og:type']/@content")?.Value ?? string.Empty;
            var image       = context.Selectable.XPath(".//meta[@property='og:image' or @property='twitter:image']/@content")?.Value ?? string.Empty;
            var locale      = context.Selectable.XPath(".//meta[@property='og:locale']/@content")?.Value ?? string.Empty;
            var body        = Regex.Replace(context.Selectable.XPath(".//body")?.Value ?? string.Empty, "[^a-zA-Z]+", " ") ?? string.Empty;
            var terms       = body?.Split(" ", StringSplitOptions.RemoveEmptyEntries).Distinct().Select(term => term.ToLower()).Where(term => term.Length > 2).ToArray() ?? default;

            var links = context.Selectable.Links().Distinct().ToArray();

            foreach (var link in links)
            {
                if (Uri.TryCreate(link, UriKind.Absolute, out var uri))
                {
                    context.AddFollowRequests(new DotnetSpider.Http.Request(uri.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                context.AddData(typeName, new EventCrawler
                {
                    Url         = url,
                    Name        = name,
                    Title       = title ?? name,
                    Favicon     = favicon,
                    Description = description,
                    Type        = type,
                    Image       = image,
                    Locale      = locale,
                    Body        = body,
                    Terms       = terms ?? Array.Empty<string>(),
                    Pts         = (!string.IsNullOrEmpty(description) ? 20 : -30) + 
                                  (!string.IsNullOrEmpty(favicon) ? 6 : -1) + 
                                  (!string.IsNullOrEmpty(image) ? 7 : -4) + 
                                  (!string.IsNullOrEmpty(locale) ? 1 : 0) + 
                                  (!string.IsNullOrEmpty(type) ? 1 : 0) + 
                                  (!string.IsNullOrEmpty(name) ? 15 : -5)
                });
            }

            return Task.CompletedTask;
        }

        public override Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }