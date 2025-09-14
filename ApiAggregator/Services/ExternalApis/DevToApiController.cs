using ApiAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DevToController : ControllerBase
{
    private readonly IExternalApiService _devToService;

    public DevToController(IExternalApiService devToService)
    {
        _devToService = devToService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? filter = "dotnet")
    {
        var data = await _devToService.GetDataAsync(filter);
        return Ok(data);
    }
}
