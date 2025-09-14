using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggregatedController : ControllerBase
    {
        private readonly AggregatedService _aggregatedService;

        public AggregatedController(AggregatedService aggregatedService)
        {
            _aggregatedService = aggregatedService;
        }
       
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? searchTerm = null, [FromQuery] DateSortOrder dateOrder = DateSortOrder.Descending, [FromQuery] List<DataSource>? dataSources = null) 
        {
            var data = await _aggregatedService.GetAggregatedDataAsync(searchTerm, dateOrder, dataSources);
            return Ok(data);
        }
    }
}
