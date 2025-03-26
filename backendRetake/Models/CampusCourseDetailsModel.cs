namespace backendRetake.Models
{
    public class CampusCourseDetailsModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int StartYear { get; set; }
        public int MaximumStudentsCount { get; set; }
        public int StudentsEnrolledCount { get; set; }
        public int StudentsInQueueCount { get; set; }
        public string Requirements { get; set; }
        public string Annotations { get; set; }
        public CourseStatuses Status { get; set; }
        public Semesters Semester { get; set; }
        public List<CampusCourseStudentModel> Students { get; set; }
        public List<CampusCourseTeacherModel> Teachers { get; set; }
        public List<CampusCourseNotificationModel> Notifications { get; set; }
    }
}
