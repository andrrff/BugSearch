using Serilog;
using BugSearch.Shared.Enums;
using BugSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using BugSearch.Crawler.Services;
using BugSearch.Shared.Singletons;

namespace BugSearch.Crawler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public ApiController(ILogger<ApiController> logger, IHostApplicationLifetime appLifetime)
        {
            _logger      = logger;
            _appLifetime = appLifetime;
        }

        [HttpPost("task/create", Name = "Spider")]
        public async Task<ActionResult<CrawlerJob>> PostAsync([FromBody] CrawlerRequest req)
        {
            try
            {
                var jobTask        = new CrawlerJob(JobStatus.Waiting, req.Urls, new CancellationTokenSource());
                var crawlerService = new CrawlerService(req, jobTask);
                Log.Logger.Information("{jobTask} - Spider (Crawler)", jobTask.JobId.Replace(Environment.NewLine, string.Empty));

                TaskJobs.GetInstance().AddJob(jobTask);
                await crawlerService.StartAsync(jobTask.CancellationTokenSource.Token);

                return new JsonResult(jobTask);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error on Spider (Crawler)");
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.NotFound());
        }


        [HttpGet("task/list", Name = "GetSpiderList")]
        public ActionResult<IEnumerable<CrawlerJob>> GetSpiderList()
        {
            try
            {
                var spiderList = TaskJobs.GetInstance().GetJobs();
                Log.Logger.Information("GetSpiderList (Crawler)");

                return new JsonResult(spiderList);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error on GetSpiderList");
            }

            Log.CloseAndFlush();

            return new JsonResult(new List<CrawlerJob>());
        }

        [HttpGet("task/{jobId}/", Name = "GetJobStatus")]
        public ActionResult<CrawlerJob> GetJobStatus(string jobId)
        {
            try
            {
                var jobTask = TaskJobs.GetInstance().GetJob(jobId);

                if (jobTask is not default(CrawlerJob))
                {
                    var isRunning = !jobTask.CancellationTokenSource.Token.IsCancellationRequested;

                    if (isRunning)
                    {
                        jobTask.Status = JobStatus.Running;
                        TaskJobs.GetInstance().UpdateJob(jobTask);

                        Log.Logger.Information("{jobId} - GetJobStatus (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));

                        return new JsonResult(jobTask);
                    }
                    else
                    {
                        return new JsonResult(jobTask);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "{jobId} - Error on GetJobStatus (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.NotFound());
        }

        [HttpPatch("task/{jobId}/cancel", Name = "CancelJob")]
        public ActionResult<CrawlerJob> CancelJob(string jobId)
        {
            try
            {
                var jobTask = TaskJobs.GetInstance().GetJob(jobId);

                if (jobTask is not default(CrawlerJob))
                {
                    var cancellationTokenSource = jobTask.CancellationTokenSource;

                    if (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        cancellationTokenSource.Cancel();

                        jobTask.Status = JobStatus.Canceled;
                        jobTask.Message = "O trabalho foi cancelado com sucesso.";
                        TaskJobs.GetInstance().UpdateJob(jobTask);

                        Log.Logger.Information("{jobId} - CancelJob (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));

                        return new JsonResult(jobTask);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "{jobId} - Error on CancelJob (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.NotFound());
        }

        [HttpPatch("task/cancel", Name = "CancelJobs")]
        public ActionResult<IEnumerable<CrawlerJob>> CancelJobs()
        {
            try
            {
                var jobTasks = TaskJobs.GetInstance().GetJobs();

                foreach (var jobTask in jobTasks)
                {
                    var cancellationTokenSource = jobTask.CancellationTokenSource;

                    if (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        cancellationTokenSource.Cancel();

                        jobTask.Status = JobStatus.Canceled;
                        jobTask.Message = "O trabalho foi cancelado com sucesso.";
                        TaskJobs.GetInstance().UpdateJob(jobTask);

                        Log.Logger.Information("All - CancelJobs (Crawler)");
                    }
                }

                return new JsonResult(jobTasks);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error on CancelJobs (Crawler)");
            }

            Log.CloseAndFlush();

            return new JsonResult(new List<CrawlerJob>());
        }

        [HttpDelete("task/{jobId}/delete", Name = "DeleteJob")]
        public ActionResult<CrawlerJob> DeleteJob(string jobId)
        {
            try
            {
                var jobTask = TaskJobs.GetInstance().GetJob(jobId);

                if (jobTask is not default(CrawlerJob))
                {
                    var cancellationTokenSource = jobTask.CancellationTokenSource;

                    if (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        cancellationTokenSource.Cancel();

                        jobTask.Status = JobStatus.Canceled;
                        jobTask.Message = "O trabalho foi cancelado com sucesso.";
                        TaskJobs.GetInstance().UpdateJob(jobTask);

                        Log.Logger.Information("{jobId} - DeleteJob (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
                    }

                    TaskJobs.GetInstance().RemoveJob(jobId);

                    return new JsonResult(jobTask);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "{jobId} - Error on DeleteJob (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.NotFound());
        }

        [HttpDelete("task/delete", Name = "DeleteJobs")]
        public ActionResult<IEnumerable<CrawlerJob>> DeleteJobs()
        {
            try
            {
                var jobTasks = TaskJobs.GetInstance().GetJobs();

                foreach (var jobTask in jobTasks)
                {
                    var cancellationTokenSource = jobTask.CancellationTokenSource;

                    if (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        cancellationTokenSource.Cancel();

                        jobTask.Status = JobStatus.Canceled;
                        jobTask.Message = "O trabalho foi cancelado com sucesso.";
                        TaskJobs.GetInstance().UpdateJob(jobTask);

                        Log.Logger.Information("All - DeleteJobs (Crawler)");
                    }
                }

                TaskJobs.GetInstance().RemoveAllJobs();

                return new JsonResult(jobTasks);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error on DeleteJobs (Crawler)");
            }

            Log.CloseAndFlush();

            return new JsonResult(new List<CrawlerJob>());
        }
    }
}