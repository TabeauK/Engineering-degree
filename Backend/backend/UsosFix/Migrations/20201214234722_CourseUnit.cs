using Microsoft.EntityFrameworkCore.Migrations;

namespace UsosFix.Migrations
{
    public partial class CourseUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsosId",
                table: "Subjects");

            migrationBuilder.AddColumn<string>(
                name: "UsosUnitId",
                table: "Groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsosUnitId",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "UsosId",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
