using BugSearch.Shared.Enums;
using BugSearch.Shared.Models;

namespace BugSearch.Crawler.Services
{
    public class CrawlerService : BackgroundService
    {
        private readonly IEnumerable<string> _urls;
        private readonly CrawlerJob _jobTask;
        private readonly CancellationToken _cancellationToken;

        public CrawlerService(IEnumerable<string> urls, CrawlerJob jobTask)
        {
            _urls              = urls ?? throw new ArgumentNullException(nameof(urls));
            _jobTask           = jobTask ?? throw new ArgumentNullException(nameof(jobTask));
            _cancellationToken = jobTask.CancellationTokenSource.Token;
        }

        public CrawlerJob GetJobTask()
        {
            return _jobTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _jobTask.Status = JobStatus.Running;

            try
            {
                RobotSingleton.GetInstance().SetUrls(_urls);
                await DistributedSpider.RunAsync(stoppingToken);
                _jobTask.Status = JobStatus.Completed;
            }
            catch (OperationCanceledException)
            {
                _jobTask.Status = stoppingToken.IsCancellationRequested ? JobStatus.RequestedCancel : JobStatus.Canceled;
                _jobTask.Message = "O trabalho foi cancelado.";
            }
            catch (Exception ex)
            {
                _jobTask.Status = JobStatus.Error;
                _jobTask.Message = ex.Message;
            }
        }
    }
}
