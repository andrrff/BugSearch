using BugSearch.Crawler.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugSearch.Crawler.Controllers;

[ApiController]
[Route("[controller]/robot")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;

    public ApiController(ILogger<ApiController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "Spider")]
    public async Task PostAsync([FromBody] IEnumerable<string> url)
    {
        RobotSingleton.GetInstance().SetUrls(url);
        await DistributedSpider.RunAsync();
    }
}