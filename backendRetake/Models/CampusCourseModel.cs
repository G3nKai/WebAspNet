using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backendRetake.Models
{
    public class CampusCourseModel
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        [Range(2000, 2029)]
        public int StartYear { get; set; }
        [Required]
        [Range(1, 200)]
        public int MaximumStudentsCount { get; set; }
        [Required]
        public int RemainingSlotsCount { get; set; }
        [Required]
        public CourseStatuses Status { get; set; }
        [Required]
        public Semesters Semester { get; set; }
        [Required]
        public required string Requirements { get; set; }
        [Required]
        public required string Annotations { get; set; }
        [Required]
        public Guid MainTeacherId { get; set; }
        [Required]
        public Guid CampusGroupId { get; set; }
        public CampusGroupModel CampusGroup { get; set; }
        public ICollection<CampusCourseUser> CampusCourseUsers { get; set; }
        public ICollection<CampusCourseNotificationModel> CampusCourseNotifications { get; set; }
        [JsonIgnore]
        public DateTime CreationTime { get; set; }
    }
}
