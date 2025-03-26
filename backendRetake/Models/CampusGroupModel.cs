using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backendRetake.Models
{
    public class CampusGroupModel
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Campus group name is required.")]
        public string Name { get; set; }
        [JsonIgnore]
        public List<CampusCourseModel> CampusCourses { get; set; }

    }
}
