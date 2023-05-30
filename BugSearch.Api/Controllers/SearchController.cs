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
    public SearchResult Get([FromQuery] string q, [FromQuery] int l = 20)
    {
        return _context.FindWebSites(q, l);
    }
}