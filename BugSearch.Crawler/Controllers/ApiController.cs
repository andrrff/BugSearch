using Serilog;
using System.Net;
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

        /// <summary>
        /// Cria um novo trabalho de crawler.
        /// </summary>
        /// <param name="req">Lista de urls</param>
        /// <returns></returns>
        /// <response code="201">Retorna o trabalho criado</response>
        /// <response code="400">Se houver algum erro</response>
        [HttpPost("task/create", Name = "Spider")]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<CrawlerJob>> PostAsync([FromBody] CrawlerRequest req)
        {
            try
            {
                var jobTask        = new CrawlerJob(JobStatus.Waiting, req.Urls, new CancellationTokenSource());
                var crawlerService = new CrawlerService(req, jobTask);
                Log.Logger.Information("{jobTask} - Spider (Crawler)", jobTask.JobId.Replace(Environment.NewLine, string.Empty));

                TaskJobs.GetInstance().AddJob(jobTask);
                await crawlerService.StartAsync(jobTask.CancellationTokenSource.Token);

                return new JsonResult(jobTask)
                {
                    StatusCode = (int)HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error on Spider (Crawler)");
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.BadRequest())
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }


        /// <summary>
        /// Retorna uma lista de trabalhos de crawler.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Retorna a lista de trabalhos</response>
        /// <response code="204">Se a lista estiver vazia</response>
        /// <response code="400">Se houver algum erro</response>
        [HttpGet("task/list", Name = "GetSpiderList")]
        [ProducesResponseType(typeof(IEnumerable<CrawlerJob>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(IEnumerable<CrawlerJob>), (int)HttpStatusCode.BadRequest)]
        public ActionResult<IEnumerable<CrawlerJob>> GetSpiderList()
        {
            try
            {
                var spiderList = TaskJobs.GetInstance().GetJobs();
                Log.Logger.Information("GetSpiderList (Crawler)");

                if (spiderList is not default(List<CrawlerJob>) && 
                    spiderList.Count() > 0)
                {
                    return new JsonResult(spiderList);
                }
                else
                {
                    return new JsonResult(new List<CrawlerJob>())
                    {
                        StatusCode = (int)HttpStatusCode.NoContent
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error on GetSpiderList");
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.BadRequest())
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        /// <summary>
        /// Retorna um trabalho de crawler.
        /// </summary>
        /// <param name="jobId" example="0d2625dd-dae1-4d6f-89c1-763a8af4ca3e">Id do trabalho</param>
        /// <returns></returns>
        /// <response code="200">Retorna o trabalho</response>
        /// <response code="406">Se o trabalho não estiver em execução</response>
        /// <response code="404">Se o trabalho não for encontrado</response>
        /// <response code="400">Se houver algum erro</response>
        [HttpGet("task/{jobId}/", Name = "GetJobStatus")]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.NotAcceptable)]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.BadRequest)]
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
                        return new JsonResult(jobTask)
                        {
                            StatusCode = (int)HttpStatusCode.NotAcceptable
                        };
                    }
                }
                else
                {
                    return new JsonResult(CrawlerJob.NotFound())
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "{jobId} - Error on GetJobStatus (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.BadRequest())
            {   
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        /// <summary>
        /// Cancela um trabalho de crawler.
        /// </summary>
        /// <param name="jobId" example="0d2625dd-dae1-4d6f-89c1-763a8af4ca3e">Id do trabalho</param>
        /// <returns></returns>
        /// <response code="200">Retorna o trabalho cancelado</response>
        /// <response code="404">Se o trabalho não for encontrado</response>
        /// <response code="406">Se o trabalho não estiver em execução</response>
        /// <response code="400">Se houver algum erro</response>
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
                    else
                    {
                        return new JsonResult(jobTask)
                        {
                            StatusCode = (int)HttpStatusCode.NotAcceptable
                        };
                    }
                }
                else
                {
                    return new JsonResult(CrawlerJob.NotFound())
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "{jobId} - Error on CancelJob (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.BadRequest())
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        /// <summary>
        /// Cancela todos os trabalhos de crawler.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Retorna a lista de trabalhos cancelados</response>
        /// <response code="400">Se houver algum erro</response>
        [HttpPatch("task/cancel", Name = "CancelJobs")]
        [ProducesResponseType(typeof(IEnumerable<CrawlerJob>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CrawlerJob>), (int)HttpStatusCode.BadRequest)]
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

            return new JsonResult(CrawlerJob.BadRequest())
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        /// <summary>
        /// Deleta um trabalho de crawler.
        /// </summary>
        /// <param name="jobId" example="0d2625dd-dae1-4d6f-89c1-763a8af4ca3e">Id do trabalho</param>
        /// <returns></returns>
        /// <response code="200">Retorna o trabalho deletado</response>
        /// <response code="404">Se o trabalho não for encontrado</response>
        /// <response code="400">Se houver algum erro</response>
        [HttpDelete("task/{jobId}/delete", Name = "DeleteJob")]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(CrawlerJob), (int)HttpStatusCode.BadRequest)]
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
                else
                {
                    return new JsonResult(CrawlerJob.NotFound())
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "{jobId} - Error on DeleteJob (Crawler)", jobId.Replace(Environment.NewLine, string.Empty));
            }

            Log.CloseAndFlush();

            return new JsonResult(CrawlerJob.BadRequest())
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        /// <summary>
        /// Cancela todos os trabalhos de crawler.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Retorna a lista de trabalhos cancelados</response>
        /// <response code="400">Se houver algum erro</response>
        [HttpDelete("task/delete", Name = "DeleteJobs")]
        [ProducesResponseType(typeof(IEnumerable<CrawlerJob>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CrawlerJob>), (int)HttpStatusCode.BadRequest)]
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

            return new JsonResult(CrawlerJob.BadRequest())
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }
    }
}