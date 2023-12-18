using System.ComponentModel.DataAnnotations;

namespace SecuringWebAPI.Model.DTO
{
    public class RegistrationModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set;}
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }

    }
}
