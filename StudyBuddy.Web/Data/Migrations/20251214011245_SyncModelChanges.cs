using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StudyTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskType",
                table: "StudyTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "StudyTasks");

            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "StudyTasks");
        }
    }
}
