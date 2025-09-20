using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class FileUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string Owner { get; set; }
    }
}
