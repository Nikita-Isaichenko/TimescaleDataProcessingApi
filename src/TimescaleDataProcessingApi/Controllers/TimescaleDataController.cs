using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
