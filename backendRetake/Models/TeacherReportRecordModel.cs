using Microsoft.AspNetCore.Mvc;

namespace backendRetake.Models
{
    public class TeacherReportRecordModel
    {
        public string FullName { get; set; }
        public Guid Id { get; set; }
        public List<CampusGroupReportModel> CampusGroupReports { get; set; }
    }
}
