using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpdateController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UpdateController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFile([FromForm] FileUpdateRequest request)
        {
            var File = request.File;
            var Owner = request.Owner;
            var FileName = request.FileName;

            if (File == null || string.IsNullOrWhiteSpace(Owner) || string.IsNullOrWhiteSpace(FileName))
                return BadRequest("File, Owner and FileName are required.");

            if (!File.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(File.FileName).ToLower() != ".jpg")
                return BadRequest("File must be a JPG image.");

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadPath = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadPath);

            var imagePath = Path.Combine(uploadPath, FileName);
            var jsonPath = Path.Combine(uploadPath, $"{Path.GetFileNameWithoutExtension(FileName)}.json");

            if (!System.IO.File.Exists(imagePath) || !System.IO.File.Exists(jsonPath))
                return BadRequest("File does not exist. Use Create to add new images.");

            try
            {
             
                var jsonContent = await System.IO.File.ReadAllTextAsync(jsonPath);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (!metadata.ContainsKey("Owner") || metadata["Owner"] != Owner)
                    return Forbid("Owner mismatch.");

               
                using var stream = System.IO.File.Create(imagePath);
                await File.CopyToAsync(stream);

            
                metadata["LastModificationTime"] = System.IO.File.GetLastWriteTimeUtc(imagePath).ToString("o");
                var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(jsonPath, updatedJson);

                return Ok("Updated");
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
