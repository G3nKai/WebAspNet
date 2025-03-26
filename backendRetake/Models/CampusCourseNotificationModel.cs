using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backendRetake.Models
{
    public class CampusCourseNotificationModel
    {
        [JsonIgnore]
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Text { get; set; }
        public bool IsImportant { get; set; }
        [JsonIgnore]
        [Required]
        public Guid CampusCourseId { get; set; }
        [JsonIgnore]
        public CampusCourseModel CampusCourse { get; set; }
    }
}
