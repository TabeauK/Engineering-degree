using Microsoft.EntityFrameworkCore.Migrations;

namespace UsosFix.Migrations
{
    public partial class ExchangeUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Exchanges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Exchanges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_SubjectId",
                table: "Exchanges",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exchanges_Subjects_SubjectId",
                table: "Exchanges",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exchanges_Subjects_SubjectId",
                table: "Exchanges");

            migrationBuilder.DropIndex(
                name: "IX_Exchanges_SubjectId",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Exchanges");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Exchanges");
        }
    }
}
