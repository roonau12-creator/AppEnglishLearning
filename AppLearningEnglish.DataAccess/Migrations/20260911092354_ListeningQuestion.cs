using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ListeningQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListeningQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListeningLessonId = table.Column<int>(type: "integer", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListeningQuestions_ListeningLessons_ListeningLessonId",
                        column: x => x.ListeningLessonId,
                        principalTable: "ListeningLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningQuestions_ListeningLessonId",
                table: "ListeningQuestions",
                column: "ListeningLessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListeningQuestions");
        }
    }
}
