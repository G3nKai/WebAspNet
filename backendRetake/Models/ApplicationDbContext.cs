using Microsoft.EntityFrameworkCore;

namespace backendRetake.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) 
        {
            Database.EnsureCreated();
        }

        public DbSet<CampusGroupModel> CampusGroup { get; set; }
        public DbSet<UserModel> User { get; set; }
        public DbSet<LogoutToken> TokenBlackListed {  get; set; } 
        public DbSet<CampusCourseModel> CampusCourse { get; set; }
        public DbSet<CampusCourseUser> CampusCourseUser { get; set; }
        public DbSet<CampusCourseNotificationModel> Notification { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CampusCourseModel>()
                .Property(c => c.Status)
                .HasConversion<string>();
            modelBuilder.Entity<CampusCourseModel>()
                .Property(c => c.Semester)
                .HasConversion<string>();

            //modelBuilder.Entity<CampusCourseStudentModel>()
            //    .Property(c => c.Status)
            //    .HasConversion<string>();
            //modelBuilder.Entity<CampusCourseStudentModel>()
            //    .Property(c => c.MidtermResult)
            //    .HasConversion<string>();
            //modelBuilder.Entity<CampusCourseStudentModel>()
            //    .Property(c => c.FinalResult)
            //    .HasConversion<string>();

            modelBuilder.Entity<CampusCourseModel>()
                .HasOne(c => c.CampusGroup)
                .WithMany(g => g.CampusCourses)
                .HasForeignKey(c => c.CampusGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampusCourseUser>()
                .HasOne(c => c.CampusCourse)
                .WithMany(g => g.CampusCourseUsers)
                .HasForeignKey(c => c.CampusCourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CampusCourseUser>()
                .HasOne(c => c.User)
                .WithMany(g => g.CampusCourseUsers)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CampusCourseUser>()
                .Property(c => c.Role)
                .HasConversion<string>();
            modelBuilder.Entity<CampusCourseUser>()
                .Property(c => c.FinalResult)
                .HasConversion<string>();
            modelBuilder.Entity<CampusCourseUser>()
                .Property(c => c.MidtermResult)
                .HasConversion<string>();
            modelBuilder.Entity<CampusCourseUser>()
                .Property(c => c.Status)
                .HasConversion<string>();

            modelBuilder.Entity<CampusCourseNotificationModel>()
                .HasOne(n => n.CampusCourse)
                .WithMany(c => c.CampusCourseNotifications)
                .HasForeignKey(n => n.CampusCourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
