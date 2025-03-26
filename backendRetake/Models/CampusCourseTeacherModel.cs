using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class CampusCourseTeacherModel
    {
        public string Name {  get; set; }
        public string Email { get; set; }
        public bool? isMain { get; set; }
    }
}
