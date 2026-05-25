using Microsoft.AspNetCore.Http;

namespace Saowari.Models.DTOs.User
{
    public class UserUpdateDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Picture { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}