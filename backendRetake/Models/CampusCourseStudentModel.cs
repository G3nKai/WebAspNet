using System.ComponentModel.DataAnnotations;

namespace backendRetake.Models
{
    public class CampusCourseStudentModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public StudentStatuses? Status { get; set; }
        public StudentMarks? MidtermResult { get; set; }
        public StudentMarks? FinalResult { get; set; }
    }
}
