using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserLessonStepsAndAttemptAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExerciseDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ListeningDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VocabDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAnswersJson",
                table: "ExerciseAttempts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseDone",
                table: "UserLessons");

            migrationBuilder.DropColumn(
                name: "ListeningDone",
                table: "UserLessons");

            migrationBuilder.DropColumn(
                name: "VocabDone",
                table: "UserLessons");

            migrationBuilder.DropColumn(
                name: "SelectedAnswersJson",
                table: "ExerciseAttempts");
        }
    }
}
