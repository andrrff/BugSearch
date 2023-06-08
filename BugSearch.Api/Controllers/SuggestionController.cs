using Serilog;
using Microsoft.AspNetCore.Mvc;
using BugSearch.Shared.Services;
using System.Net;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SuggestionController : ControllerBase
{
    private readonly DatabaseConntection _context;

    public SuggestionController()
    {
        _context = new DatabaseConntection();
    }

    /// <summary>
    /// Get suggestion for query
    /// </summary>
    /// <param name="q" example="t4rmo quitro">Query</param>
    /// <returns></returns>
    /// <response code="200">Returns sugestion for query</response>
    /// <response code="400">If the query is null</response>
    [HttpGet(Name = "GetSugestion")]
    [ ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public ActionResult<string> GetAsync([FromQuery]string q)
    {
        try
        {
            var sugestedQuery = _context.SugestedQueryByLevenshtein(q);
            Log.Logger.Information("GetAsync Sugestion: {Query} (API)", sugestedQuery.Replace(Environment.NewLine, ""));

            return new OkObjectResult(sugestedQuery);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error on GetAsync Sugestion (API)");
        }

        Log.CloseAndFlush();

        return new BadRequestResult();
    }
    
    
}