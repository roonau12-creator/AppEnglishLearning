using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class StudySettingsAndCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateCode",
                table: "UserCourses",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "ListeningQuestions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "ListeningQuestions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "FillBlank");

            migrationBuilder.AddColumn<string>(
                name: "TextAnswersJson",
                table: "ExerciseAttempts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyGoalMinutes",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<int>(
                name: "ReviewIntervalPercent",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "VocabDailyTarget",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateCode",
                table: "UserCourses");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "ListeningQuestions");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "ListeningQuestions");

            migrationBuilder.DropColumn(
                name: "TextAnswersJson",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "DailyGoalMinutes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ReviewIntervalPercent",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VocabDailyTarget",
                table: "AspNetUsers");
        }
    }
}
