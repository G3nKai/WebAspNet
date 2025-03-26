using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class EditCampusCourseModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(2000, 2029)]
        public int StartYear { get; set; }
        [Required]
        [Range(1, 200)]
        public int MaximumStudentsCount { get; set; }
        [Required]
        public Semesters Semester { get; set; }
        [Required]
        public string Requirements { get; set; }
        [Required]
        public string Annotations { get; set; }
        [Required]
        public Guid MainTeacherId { get; set; }
    }
}
