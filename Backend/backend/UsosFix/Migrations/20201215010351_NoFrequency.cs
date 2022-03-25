using Microsoft.EntityFrameworkCore.Migrations;

namespace UsosFix.Migrations
{
    public partial class NoFrequency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "GroupMeetings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "GroupMeetings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
