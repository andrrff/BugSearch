using BugSearch.Api.Services;
using BugSearch.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<string> GetAsync()
    {
        try
        {
            return KubernetesClient.GetAllSecrets();
        }
        catch (Exception ex)
        {
            return ex.ToString();            
        }
    }
}