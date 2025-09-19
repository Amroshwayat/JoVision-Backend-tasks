using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BirthDateController : ControllerBase
    {
      
        [HttpPost]
        public IActionResult Post(
            [FromForm] string? name,
            [FromForm] int? years,
            [FromForm] int? months,
            [FromForm] int? days)
        {
            var personName = string.IsNullOrWhiteSpace(name) ? "anonymous" : name;


            if (years == null || months == null || days == null)
            {
                return Ok($"Hello {personName}, I can’t calculate your age without knowing your birthdate!");
            }

            try
            {
                var birthDate = new DateTime(years.Value, months.Value, days.Value);

                var today = DateTime.Today;
                int age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;

                return Ok($"Hello {personName}, your age is {age}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid date supplied: {ex.Message}");
            }
        }
    }
}
