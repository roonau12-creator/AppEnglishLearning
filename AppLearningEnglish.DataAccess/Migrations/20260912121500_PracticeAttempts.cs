using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260912121500_PracticeAttempts")]
    public partial class PracticeAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PracticeAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: true),
                    Expected = table.Column<string>(type: "text", nullable: true),
                    UserAnswer = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeAttempts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PracticeAttempts_ApplicationUserId",
                table: "PracticeAttempts",
                column: "ApplicationUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PracticeAttempts");
        }
    }
}
