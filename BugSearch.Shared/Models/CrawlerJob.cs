using BugSearch.Shared.Enums;
using System.Text.Json.Serialization;

namespace BugSearch.Shared.Models;

public class CrawlerJob
{
    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

    [JsonPropertyName("jobId")]
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("url")]
    public IEnumerable<string>? Url { get; set; }

    [JsonPropertyName("status")]
    public JobStatus Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    public CrawlerJob(JobStatus status, IEnumerable<string>? url, CancellationTokenSource cancellationTokenSource, string message = "")
    {
        Status                  = status;
        Url                     = url;
        CancellationTokenSource = cancellationTokenSource;
        Message                 = message;
    }

    public static CrawlerJob NotFound()
    {
        var jobId      = string.Empty;
        var crawlerJob = new CrawlerJob(JobStatus.Error, null, new CancellationTokenSource(), "Trabalho não encontrado.");

        crawlerJob.JobId = jobId;

        return crawlerJob;
    }
}