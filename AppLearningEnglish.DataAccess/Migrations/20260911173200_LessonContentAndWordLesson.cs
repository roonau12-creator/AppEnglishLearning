using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LessonContentAndWordLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "words",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrammarNotes",
                table: "Lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_words_LessonId",
                table: "words",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_words_Lessons_LessonId",
                table: "words",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_words_Lessons_LessonId",
                table: "words");

            migrationBuilder.DropIndex(
                name: "IX_words_LessonId",
                table: "words");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "words");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "GrammarNotes",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Lessons");
        }
    }
}
