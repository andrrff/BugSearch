using Serilog;
using BugSearch.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PromptController : ControllerBase
{
    [HttpGet]
    public async Task<string> GetAsync([FromQuery]string q)
    {
        try
        {
            Log.Logger.Information("GetAsync OpenAI.PromptSearch: {q} (API)", q);
            return await OpenAI.PromptSearch(q);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error on GetAsync OpenAI.PromptSearch (API)");
        }

        return string.Empty;
    }
}