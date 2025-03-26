using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class AddCampusCourseNotificationModel
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public bool isImportant { get; set; }
    }
}
