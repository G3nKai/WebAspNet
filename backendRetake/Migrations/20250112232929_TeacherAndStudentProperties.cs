using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class TeacherAndStudentProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinalResult",
                table: "CampusCourseUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MidtermResult",
                table: "CampusCourseUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CampusCourseUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isMain",
                table: "CampusCourseUser",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalResult",
                table: "CampusCourseUser");

            migrationBuilder.DropColumn(
                name: "MidtermResult",
                table: "CampusCourseUser");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CampusCourseUser");

            migrationBuilder.DropColumn(
                name: "isMain",
                table: "CampusCourseUser");
        }
    }
}
