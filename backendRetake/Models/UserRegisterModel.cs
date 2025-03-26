using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class UserRegisterModel
    {
        [Required]
        public required string FullName { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "The password should be at least 6 characters.")]
        [MaxLength(32, ErrorMessage = "The password should not exceed 32 characters limit.")]
        public required string Password { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "The password should be at least 6 characters.")]
        [MaxLength(32, ErrorMessage = "The password should not exceed 32 characters limit.")]
        public required string ConfirmPassword { get; set; }
    }
}
