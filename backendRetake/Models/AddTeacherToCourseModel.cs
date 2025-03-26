using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class AddTeacherToCourseModel
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
