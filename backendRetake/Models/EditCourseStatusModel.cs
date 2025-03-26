using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class EditCourseStatusModel
    {
        [Required]
        public CourseStatuses Status { get; set; }
    }
}
