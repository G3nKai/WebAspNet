using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isImportant",
                table: "Notification",
                newName: "IsImportant");

            migrationBuilder.AddColumn<Guid>(
                name: "CampusCourseId",
                table: "Notification",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CampusCourseId",
                table: "Notification",
                column: "CampusCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_CampusCourse_CampusCourseId",
                table: "Notification",
                column: "CampusCourseId",
                principalTable: "CampusCourse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_CampusCourse_CampusCourseId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_CampusCourseId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "CampusCourseId",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "IsImportant",
                table: "Notification",
                newName: "isImportant");
        }
    }
}
