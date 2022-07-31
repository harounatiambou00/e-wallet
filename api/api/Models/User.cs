using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class User
    {
        public  Int64 UserId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "The first name must be less thn 50 characters")]
        public String FirstName { get; set; } = String.Empty;

        [Required]
        [MaxLength(50, ErrorMessage ="The last name must be less thn 50 characters")]
        public String LastName { get; set; } = String.Empty ;

        [Required]
        [MaxLength(100, ErrorMessage = "The email must be less thn 100 characters")]
        public String Email { get; set; } = String.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];

        //When a usser register, we'll create a verification token to verify the email.
        public String? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public String? PasswordResetToken { get; set; }
        //The expiration date for the Reset token.
        public DateTime? ResetTokenExpires { get; set; }

        public String Devise { get; set; } = "FCFA";
        public DateTime SigneUpDate { get; set; }

        public static implicit operator User(ServiceResponse<User?> v)
        {
            throw new NotImplementedException();
        }
    }
}
