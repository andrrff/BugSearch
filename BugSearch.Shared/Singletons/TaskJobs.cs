using BugSearch.Shared.Models;

namespace BugSearch.Shared.Singletons;

public class TaskJobs
{
    private TaskJobs() { }

    private static TaskJobs _instance = default!;

    private static readonly object _lock = new object();

    private IEnumerable<CrawlerJob> _jobs = new List<CrawlerJob>();

    public static TaskJobs GetInstance()
    {
        if (_instance is null)
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = new TaskJobs();
                }
            }
        }

        return _instance;
    }

    public CrawlerJob GetJob(string jobId)
    {
        return _jobs.FirstOrDefault(x => x.JobId == jobId) ?? CrawlerJob.NotFound();
    }

    public IEnumerable<CrawlerJob> GetJobs()
    {
        return _jobs;
    }

    public void AddJob(CrawlerJob job)
    {
        var jobs = _jobs;
        jobs = jobs.Append(job);
        _jobs = jobs;
    }

    public void UpdateJob(CrawlerJob job)
    {
        var jobs = _jobs;
        var index = jobs.ToList().FindIndex(x => x.JobId == job.JobId);
        jobs.ElementAt(index).Status = job.Status;
        _jobs = jobs;
    }

    public void RemoveJob(string jobId)
    {
        var jobs = _jobs;
        jobs = jobs.Where(x => x.JobId != jobId);
        _jobs = jobs;
    }

    public void RemoveAllJobs()
    {
        _jobs = new List<CrawlerJob>();
    }
}