using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class EditCampusCourseRequirementsAndAnnotationsModel
    {
        [Required]
        public string Requirements { get; set; }
        [Required]
        public string Annotations { get; set; }
    }
}
