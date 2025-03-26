using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class Annotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Annotation",
                table: "CampusCourse",
                newName: "Annotations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Annotations",
                table: "CampusCourse",
                newName: "Annotation");
        }
    }
}
