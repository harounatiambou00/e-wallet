using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class ResetPasswordDTO
    {
        [Required]
        public String Token { get; set; } = String.Empty;

        [Required, MinLength(6)]
        public String NewPassword { get; set; } = String.Empty;

        [Required, Compare("NewPassword")]
        public String ConfirmNewPassword { get; set; } = String.Empty;

    }
}
