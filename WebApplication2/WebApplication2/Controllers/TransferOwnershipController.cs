using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferOwnershipController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public TransferOwnershipController(IWebHostEnvironment env)
        {
            _env = env;
        }

      
        [HttpGet]
        public IActionResult TransferOwnership([FromQuery] string OldOwner, [FromQuery] string NewOwner)
        {
            if (string.IsNullOrWhiteSpace(OldOwner) || string.IsNullOrWhiteSpace(NewOwner))
                return BadRequest("OldOwner and NewOwner are required.");

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadPath = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(uploadPath))
                return Ok(Array.Empty<object>());

            try
            {
                var jsonFiles = Directory.GetFiles(uploadPath, "*.json");
                var newOwnerFiles = new List<object>();

                foreach (var jsonFile in jsonFiles)
                {
                    var jsonContent = System.IO.File.ReadAllText(jsonFile);
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                    if (metadata == null || !metadata.ContainsKey("Owner") || !metadata.ContainsKey("FileName"))
                        continue;

                    if (metadata["Owner"] == OldOwner)
                    {
                        metadata["Owner"] = NewOwner;
                        var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                        System.IO.File.WriteAllText(jsonFile, updatedJson);
                    }

                    if (metadata["Owner"] == NewOwner)
                        newOwnerFiles.Add(new { fileName = metadata["FileName"], owner = metadata["Owner"] });
                }

                return Ok(newOwnerFiles);
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
