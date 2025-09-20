using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeleteController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DeleteController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult DeleteFile([FromQuery] string FileName, [FromQuery] string FileOwner)
        {
            if (string.IsNullOrWhiteSpace(FileName) || string.IsNullOrWhiteSpace(FileOwner))
                return BadRequest("FileName and FileOwner are required.");

        
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadPath = Path.Combine(webRoot, "uploads");

            var imagePath = Path.Combine(uploadPath, FileName);
            var jsonPath = Path.Combine(uploadPath, $"{Path.GetFileNameWithoutExtension(FileName)}.json");

            if (!System.IO.File.Exists(imagePath))
                return BadRequest("File does not exist.");

            if (!System.IO.File.Exists(jsonPath))
                return Forbid("Metadata file not found.");

            try
            {
        
                var jsonContent = System.IO.File.ReadAllText(jsonPath);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (!metadata.ContainsKey("Owner") || metadata["Owner"] != FileOwner)
                    return Forbid("Owner mismatch.");

           
                System.IO.File.Delete(imagePath);
                System.IO.File.Delete(jsonPath);

                return Ok("Deleted");
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
