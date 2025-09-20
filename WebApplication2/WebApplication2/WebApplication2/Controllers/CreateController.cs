using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreateController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CreateController> _logger;

        public CreateController(IWebHostEnvironment env, ILogger<CreateController> logger)
        {
            _env = env;
            _logger = logger;
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var file = request.File;
            var owner = request.Owner;

            var ext = Path.GetExtension(file.FileName);
            if (!file.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(ext, ".jpg", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("الملف يجب أن يكون صورة بامتداد .jpg");
            }

            try
            {
             
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var uploadPath = Path.Combine(webRoot, "uploads");
                Directory.CreateDirectory(uploadPath);

                
                var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadPath, fileName);

              
                using var stream = System.IO.File.Create(filePath);
                await file.CopyToAsync(stream);

                var metadata = new
                {
                    Owner = owner,
                    CreationTime = System.IO.File.GetCreationTimeUtc(filePath),
                    LastModificationTime = System.IO.File.GetLastWriteTimeUtc(filePath),
                    FileName = fileName
                };

              
                var jsonPath = Path.Combine(uploadPath, $"{Path.GetFileNameWithoutExtension(fileName)}.json");
                var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(jsonPath, json);

              
                var baseUrl = $"{Request.Scheme}://{Request.Host}/uploads";
                return Created($"{baseUrl}/{fileName}", new
                {
                    ImageUrl = $"{baseUrl}/{fileName}",
                    MetadataUrl = $"{baseUrl}/{Path.GetFileNameWithoutExtension(fileName)}.json"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload failed");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
