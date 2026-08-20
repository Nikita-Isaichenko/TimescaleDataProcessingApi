using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimescaleDataProcessingApi.DTOs.Request;
using TimescaleDataProcessingApi.Services;

namespace TimescaleDataProcessingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimescaleDataController : ControllerBase
    {
        private readonly TimescaleDataProcessingService _timescaleDataProcessingService;

        public TimescaleDataController(TimescaleDataProcessingService timescaleDataProcessingService)
        {
            _timescaleDataProcessingService = timescaleDataProcessingService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadCSVFile(IFormFile file)
        {
            try
            {
                await _timescaleDataProcessingService.ProcessCsvFile(file);
                return Ok("File processed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredResults([FromQuery] FilterDto filter)
        {
            try
            {
                var results = await _timescaleDataProcessingService.GetFilteredResultsAsync(filter);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
