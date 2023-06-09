using BugSearch.Shared.Enums;
using BugSearch.Shared.Models;

namespace BugSearch.Crawler.Services
{
    public class CrawlerService : BackgroundService
    {
        private readonly CrawlerRequest _req;
        private readonly CrawlerJob _jobTask;
        private readonly CancellationToken _cancellationToken;

        public CrawlerService(CrawlerRequest req, CrawlerJob jobTask)
        {
            _req               = req ?? throw new ArgumentNullException(nameof(req));
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
                RobotSingleton.GetInstance().SetUrls(_req.Urls);
                RobotSingleton.GetInstance().PersistData = _req.Properties.PersistData;
                _jobTask.Message = "O trabalho está sendo executado.";

                if(_req.Properties.UseMessageQueue)
                {
                    await DistributedSpider.RunAsync(stoppingToken, _req.Properties.Speed, _req.Properties.Depth);
                }
                else
                {
                    await RobotSpider.RunAsync(stoppingToken, _req.Properties.Speed, _req.Properties.Depth);
                }
                
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
