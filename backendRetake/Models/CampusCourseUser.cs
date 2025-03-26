using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendRetake.Models
{
    public class CampusCourseUser
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        public Guid CampusCourseId { get; set; }
        public CampusCourseModel CampusCourse { get; set; }

        public UserCampusCourseRole Role { get; set; }
        public StudentStatuses? Status { get; set; }
        public StudentMarks? MidtermResult { get; set; }
        public StudentMarks? FinalResult { get; set; }
        public bool? isMain { get; set; }
    }
}
