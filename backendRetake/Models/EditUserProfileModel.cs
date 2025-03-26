using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class EditUserProfileModel
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
    }
}
