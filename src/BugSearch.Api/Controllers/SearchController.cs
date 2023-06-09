using Serilog;
using System.Net;
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

    /// <summary>
    /// Retorna uma lista de resultados de acordo com a query.
    /// </summary>
    /// <param name="q" example="Ultimas noticias">Query a ser enviada</param>
    /// <param name="p" example="1">Pagina atual</param>
    /// <param name="m" example="20">Quantidade de itens por pagina</param>
    /// <param name="s" example="false">Se deve buscar por sugestões</param>
    /// <returns>Lista de resultados</returns>
    /// <response code="200">Retorna a lista de resultados</response>
    [HttpGet(Name = "GetSearch")]
    [ProducesResponseType(typeof(SearchResult), (int)HttpStatusCode.OK)]
    public SearchResult Get([FromQuery] string q, [FromQuery] int p = 1, [FromQuery] int m = 20, [FromQuery] bool s = false)
    {
        var reqId = Guid.NewGuid().ToString();

        try
        {
            q = s ? _context.SugestedQueryByLevenshtein(q) : q;
            Log.Logger.Information("{RequestId} - GetSearch: {Query}, {Param}, {Mode} (API)", reqId.Replace(Environment.NewLine, string.Empty), q.Replace(Environment.NewLine, string.Empty), p, m);
            var result = _context.FindWebSites(reqId, q, p, m);
            return result.GetPage(p, m);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "{RequestId} - Error on GetSearch (API)", reqId.Replace(Environment.NewLine, string.Empty));
        }

        Log.CloseAndFlush();
        
        return new SearchResult(){
            Id = reqId,
            Query = q,
            SearchResults = new List<WebSiteInfo>(),
            Pagination = new Pagination()
            {
                CurrentPage = p,
                TotalPages = 0,
                ItemsPerPage = m
            }
        };
    }
}