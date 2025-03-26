using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class RenameCampusGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CampusGroups",
                table: "CampusGroups");

            migrationBuilder.RenameTable(
                name: "CampusGroups",
                newName: "CampusGroup");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampusGroup",
                table: "CampusGroup",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CampusGroup",
                table: "CampusGroup");

            migrationBuilder.RenameTable(
                name: "CampusGroup",
                newName: "CampusGroups");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampusGroups",
                table: "CampusGroups",
                column: "Id");
        }
    }
}
