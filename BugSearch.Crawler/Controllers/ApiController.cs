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
        public async Task<IActionResult> PostAsync([FromBody] IEnumerable<string> url)
        {
            var jobTask        = new CrawlerJob(JobStatus.Waiting, url, new CancellationTokenSource());
            var crawlerService = new CrawlerService(url, jobTask);

            TaskJobs.GetInstance().AddJob(jobTask);
            await crawlerService.StartAsync(jobTask.CancellationTokenSource.Token);

            return new JsonResult(jobTask);
        }


        [HttpGet("task/list", Name = "GetSpiderList")]
        public IActionResult GetSpiderList()
        {
            var spiderList = TaskJobs.GetInstance().GetJobs();

            return new JsonResult(spiderList);
        }

        [HttpGet("task/{jobId}/status", Name = "GetJobStatus")]
        public IActionResult GetJobStatus(string jobId)
        {
            var jobTask = TaskJobs.GetInstance().GetJob(jobId);

            if (jobTask is not default(CrawlerJob))
            {
                var isRunning = !jobTask.CancellationTokenSource.Token.IsCancellationRequested;

                if (isRunning)
                {
                    jobTask.Status = JobStatus.Running;
                    TaskJobs.GetInstance().UpdateJob(jobTask);
                    return new JsonResult(jobTask);
                }
                else
                {
                    return new JsonResult(jobTask);
                }
            }

            return new JsonResult(CrawlerJob.NotFound());
        }

        [HttpPost("task/{jobId}/cancel", Name = "CancelJob")]
        public IActionResult CancelJob(string jobId)
        {
            var jobTask = TaskJobs.GetInstance().GetJob(jobId);

            if (jobTask is not default(CrawlerJob))
            {
                var cancellationTokenSource = jobTask.CancellationTokenSource;

                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();

                    jobTask.Status = JobStatus.RequestedCancel;
                    jobTask.Message = "O cancelamento do trabalho foi solicitado.";
                    TaskJobs.GetInstance().UpdateJob(jobTask);
                    
                    return new JsonResult(jobTask);
                }
                else
                {
                    jobTask.Status = JobStatus.Canceled;
                    jobTask.Message = "O trabalho foi cancelado com sucesso.";
                    TaskJobs.GetInstance().UpdateJob(jobTask);

                    return new JsonResult(jobTask);
                }
            }

            return new JsonResult(CrawlerJob.NotFound());
        }

        [HttpPost("task/cancel", Name = "CancelJobs")]
        public IActionResult CancelJobs()
        {
            var jobTasks = TaskJobs.GetInstance().GetJobs();

            foreach (var jobTask in jobTasks)
            {
                var cancellationTokenSource = jobTask.CancellationTokenSource;

                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();

                    jobTask.Status = JobStatus.RequestedCancel;
                    jobTask.Message = "O cancelamento do trabalho foi solicitado.";
                    TaskJobs.GetInstance().UpdateJob(jobTask);
                }
                else
                {
                    jobTask.Status = JobStatus.Canceled;
                    jobTask.Message = "O trabalho foi cancelado com sucesso.";                    
                    TaskJobs.GetInstance().UpdateJob(jobTask);
                }
            }

            return new JsonResult(jobTasks);
        }
    }
}