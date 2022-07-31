using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        public String FirstName { get; set; } = String.Empty;

        [Required]
        public String LastName { get; set; } = String.Empty;

        [Required, EmailAddress]
        public String Email { get; set; } = String.Empty;

        [Required, MinLength(6)]
        public String Password { get; set; } = String.Empty;

        [Required, Compare("Password")]
        public String ConfirmPassword { get; set; } = String.Empty;

        [Required]
        public String Devise { get; set; } = "FCFA";
    }
}
