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
        return await OpenAI.PromptSearch(q);
    }
}