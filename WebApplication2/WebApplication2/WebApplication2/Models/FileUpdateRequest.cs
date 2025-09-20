
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace WebApplication2.Models
{
    public class FileUpdateRequest
    {
        public IFormFile File { get; set; }
        public string Owner { get; set; }
        public string FileName { get; set; }
    }
}