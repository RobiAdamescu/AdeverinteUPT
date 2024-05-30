using System.ComponentModel.DataAnnotations;

namespace AngularAuthAPI.Models.Dto
{
    public class UserRegister
    {
        public string? Nume { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? Facultate { get; set; }
        public string? Specializare { get; set; }
        public string? Role { get; set; }
    }
}
