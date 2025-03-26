using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class CampusCourseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampusCourseUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampusCourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusCourseUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampusCourseUser_CampusCourse_CampusCourseId",
                        column: x => x.CampusCourseId,
                        principalTable: "CampusCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampusCourseUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    isImportant = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampusCourseUser_CampusCourseId",
                table: "CampusCourseUser",
                column: "CampusCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CampusCourseUser_UserId",
                table: "CampusCourseUser",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampusCourseUser");

            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
