using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backendRetake.Models
{
    public class UserModel
    {
        [Key]
        public Guid Id { get; set; }
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
        [JsonIgnore]
        public ICollection<CampusCourseUser> CampusCourseUsers { get; set; }
    }
}
