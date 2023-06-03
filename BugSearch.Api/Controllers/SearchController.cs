using Serilog;
using BugSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using BugSearch.Shared.Services;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController : ControllerBase
{
    private readonly ILogger<SearchController> _logger;
    private readonly DatabaseConntection _context;

    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
        _context = new DatabaseConntection();
    }

    [HttpGet(Name = "GetSearch")]
    public SearchResult Get([FromQuery] string q, [FromQuery] int p = 1, [FromQuery] int m = 20)
    {
        var reqId = Guid.NewGuid().ToString();

        try
        {
            Log.Logger.Information("{id} - GetSearch: {q}, {p}, {m} (API)", reqId, q, p, m);
            var result = _context.FindWebSites(reqId, q, p, m);
            return result.GetPage(p, m);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"{reqId} - Error on GetSearch (API)");
        }
        
        return new SearchResult();
    }
}