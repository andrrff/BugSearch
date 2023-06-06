using Serilog;
using System.Net;
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

    /// <summary>
    /// Retorna um resumo de todos os sites e termos.
    /// </summary>
    /// <returns>Resumo</returns>
    /// <response code="200">Retorna o resumo</response>
    [HttpGet(Name = "GetSummary")]
    [ProducesResponseType(typeof(SummaryResult), (int)HttpStatusCode.OK)]
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

        Log.CloseAndFlush();

        return new SummaryResult();
    }
}