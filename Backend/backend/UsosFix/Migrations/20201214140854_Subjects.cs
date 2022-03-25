using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UsosFix.Migrations
{
    public partial class Subjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MinMembers",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "Lecturers",
                table: "Groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lecturers",
                table: "Groups");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Groups",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Groups",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinMembers",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
