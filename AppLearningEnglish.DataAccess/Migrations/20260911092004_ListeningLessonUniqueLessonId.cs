using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ListeningLessonUniqueLessonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListeningLessons_LessonId",
                table: "ListeningLessons");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningLessons_LessonId",
                table: "ListeningLessons",
                column: "LessonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListeningLessons_LessonId",
                table: "ListeningLessons");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningLessons_LessonId",
                table: "ListeningLessons",
                column: "LessonId");
        }
    }
}
