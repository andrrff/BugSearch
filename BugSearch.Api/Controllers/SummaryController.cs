using Serilog;
using BugSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using BugSearch.Shared.Services;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SummaryController : ControllerBase
{
    private readonly ILogger<SummaryController> _logger;

    private readonly DatabaseConntection _context;

    public SummaryController(ILogger<SummaryController> logger)
    {
        _logger = logger;
        _context = new DatabaseConntection();
    }

    [HttpGet(Name = "GetSummary")]
    public SummaryResult Get()
    {
        try
        {
            Log.Logger.Information("GetSummary (API)");
            return _context.GetSummary();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error on GetSummary (API)");
        }

        return new SummaryResult();
    }
}