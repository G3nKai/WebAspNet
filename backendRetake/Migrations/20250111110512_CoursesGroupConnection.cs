using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendRetake.Migrations
{
    /// <inheritdoc />
    public partial class CoursesGroupConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampusGroupId",
                table: "CampusCourse",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CampusCourse_CampusGroupId",
                table: "CampusCourse",
                column: "CampusGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampusCourse_CampusGroup_CampusGroupId",
                table: "CampusCourse",
                column: "CampusGroupId",
                principalTable: "CampusGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampusCourse_CampusGroup_CampusGroupId",
                table: "CampusCourse");

            migrationBuilder.DropIndex(
                name: "IX_CampusCourse_CampusGroupId",
                table: "CampusCourse");

            migrationBuilder.DropColumn(
                name: "CampusGroupId",
                table: "CampusCourse");
        }
    }
}
