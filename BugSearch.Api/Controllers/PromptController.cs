using Serilog;
using System.Net;
using BugSearch.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PromptController : ControllerBase
{
    /// <summary>
    /// Envia uma query para o OpenAI e retorna uma resposta.
    /// </summary>
    /// <param name="q" example="Ping!">Query a ser enviada</param>
    /// <returns>Resposta</returns>
    /// <response code="200">Retorna a resposta da query</response>
    [HttpGet]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<string> GetAsync([FromQuery]string q)
    {
        try
        {
            Log.Logger.Information("GetAsync OpenAI.PromptSearch: {Query} (API)", q.Replace(Environment.NewLine, ""));

            return await OpenAI.PromptSearch(q);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error on GetAsync OpenAI.PromptSearch (API)");
        }

        Log.CloseAndFlush();

        return string.Empty;
    }
}