namespace backendRetake.Models
{
    public class CampusGroupReportModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public int AveragePassed { get; set; }
        public int AverageFailed { get; set; }
    }
}
