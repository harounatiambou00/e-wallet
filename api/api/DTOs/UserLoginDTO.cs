using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class UserLoginDTO
    {
        [Required, EmailAddress]
        public String Email { get; set; } = String.Empty;

        [Required]
        public String Password { get; set; } = String.Empty;

        [Required]
        public bool RemenberMe { get; set; } = false;
    }
}
