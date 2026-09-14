using System;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260914090000_LearnerTools")]
    public partial class LearnerTools : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UiLanguage",
                table: "AspNetUsers",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeeklySummaryAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UiLanguage", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LastWeeklySummaryAt", table: "AspNetUsers");
        }
    }
}
