using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class CampusCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampusCourse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    MaximumStudentsCount = table.Column<int>(type: "integer", nullable: false),
                    RemainingSlotsCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Semester = table.Column<string>(type: "text", nullable: false),
                    Requirements = table.Column<string>(type: "text", nullable: false),
                    Annotation = table.Column<string>(type: "text", nullable: false),
                    MainTeacherId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusCourse", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampusCourse");
        }
    }
}
