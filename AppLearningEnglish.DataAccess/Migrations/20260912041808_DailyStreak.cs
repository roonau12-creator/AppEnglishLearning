using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DailyStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyStreaks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    StudyDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MinutesStudied = table.Column<int>(type: "integer", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyStreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyStreaks_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyStreaks_ApplicationUserId_StudyDate",
                table: "DailyStreaks",
                columns: new[] { "ApplicationUserId", "StudyDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyStreaks");
        }
    }
}
