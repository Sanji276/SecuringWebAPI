using System.ComponentModel.DataAnnotations;

namespace SecuringWebAPI.Model.DTO
{
    public class RegistrationModel
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }

    }
}
