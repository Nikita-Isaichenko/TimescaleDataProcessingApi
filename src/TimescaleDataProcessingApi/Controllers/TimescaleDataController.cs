using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
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

        [HttpPost("UploadCsvFile")]
        public async Task<IActionResult> UploadCsvFile(IFormFile file)
        {
            try
            {
                await _timescaleDataProcessingService.ProcessCsvFile(file);
                return Ok("File processed");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("results")]
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

        [HttpGet("values/last-ten")]
        public async Task<IActionResult> GetLastTenValues([FromQuery] string fileName)
        {
            try
            {
                var values = await _timescaleDataProcessingService.GetLastTenValuesAsync(fileName);
                return Ok(values);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
