using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilterController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public FilterController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult FilterFiles([FromForm] FilterRequest request)
        {
            if (request.FilterType == null)
                return BadRequest("FilterType is required.");

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadPath = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(uploadPath))
                return Ok(Array.Empty<object>());

            try
            {
                var jsonFiles = Directory.GetFiles(uploadPath, "*.json");
                var result = new List<object>();

                foreach (var jsonFile in jsonFiles)
                {
                    var jsonContent = System.IO.File.ReadAllText(jsonFile);
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                    if (metadata == null || !metadata.ContainsKey("FileName") || !metadata.ContainsKey("Owner"))
                        continue;

                    DateTime creationTime = DateTime.Parse(metadata.GetValueOrDefault("CreationTime"));
                    DateTime modificationTime = DateTime.Parse(metadata.GetValueOrDefault("LastModificationTime"));
                    string owner = metadata.GetValueOrDefault("Owner");
                    string fileName = metadata.GetValueOrDefault("FileName");

                    switch (request.FilterType)
                    {
                        case FilterType.ByModificationDate:
                            if (request.ModificationDate == null)
                                return BadRequest("ModificationDate is required for this filter.");
                            if (modificationTime < request.ModificationDate)
                                result.Add(new { fileName, owner });
                            break;

                        case FilterType.ByCreationDateDescending:
                            if (request.CreationDate == null)
                                return BadRequest("CreationDate is required for this filter.");
                            if (creationTime > request.CreationDate)
                                result.Add(new { fileName, owner });
                            break;

                        case FilterType.ByCreationDateAscending:
                            if (request.CreationDate == null)
                                return BadRequest("CreationDate is required for this filter.");
                            if (creationTime > request.CreationDate)
                                result.Add(new { fileName, owner });
                            break;

                        case FilterType.ByOwner:
                            if (string.IsNullOrWhiteSpace(request.Owner))
                                return BadRequest("Owner is required for this filter.");
                            if (owner == request.Owner)
                                result.Add(new { fileName, owner });
                            break;

                        default:
                            return BadRequest("Invalid FilterType.");
                    }
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
