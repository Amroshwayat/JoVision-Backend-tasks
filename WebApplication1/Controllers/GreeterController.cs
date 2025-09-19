using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreeterController : ControllerBase
    {
     
        [HttpGet]
        public IActionResult Get([FromQuery] string? name)
        {
        
            var personName = string.IsNullOrWhiteSpace(name) ? "anonymous" : name;

        
            return Ok($"Hello {personName}");
        }
    }
}
