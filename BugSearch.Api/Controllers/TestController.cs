using BugSearch.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using BugSearch.Shared.Services;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    private readonly DatabaseConntection _context;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
        _context = new DatabaseConntection();
    }

    [HttpGet(Name = "Test env")]
    public string Get()
    {
        try
        {
            return Environment.GetEnvironmentVariable("OPENAI_KEY") ?? string.Empty;
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }
}