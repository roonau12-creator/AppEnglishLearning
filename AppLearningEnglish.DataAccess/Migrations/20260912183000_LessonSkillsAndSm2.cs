using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260912183000_LessonSkillsAndSm2")]
    public partial class LessonSkillsAndSm2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReadingDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpeakingDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WritingDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GrammarDone",
                table: "UserLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "EaseFactor",
                table: "UserVocabularies",
                type: "double precision",
                nullable: false,
                defaultValue: 2.5);

            migrationBuilder.AddColumn<int>(
                name: "IntervalDays",
                table: "UserVocabularies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Repetition",
                table: "UserVocabularies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReadingPassageId",
                table: "Lessons",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrammarTopicId",
                table: "Lessons",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WritingPromptId",
                table: "Lessons",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlacementItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ReadingPassageId",
                table: "Lessons",
                column: "ReadingPassageId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_GrammarTopicId",
                table: "Lessons",
                column: "GrammarTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_WritingPromptId",
                table: "Lessons",
                column: "WritingPromptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_ReadingPassages_ReadingPassageId",
                table: "Lessons",
                column: "ReadingPassageId",
                principalTable: "ReadingPassages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_GrammarTopics_GrammarTopicId",
                table: "Lessons",
                column: "GrammarTopicId",
                principalTable: "GrammarTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_WritingPrompts_WritingPromptId",
                table: "Lessons",
                column: "WritingPromptId",
                principalTable: "WritingPrompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Lessons_ReadingPassages_ReadingPassageId", table: "Lessons");
            migrationBuilder.DropForeignKey(name: "FK_Lessons_GrammarTopics_GrammarTopicId", table: "Lessons");
            migrationBuilder.DropForeignKey(name: "FK_Lessons_WritingPrompts_WritingPromptId", table: "Lessons");
            migrationBuilder.DropIndex(name: "IX_Lessons_ReadingPassageId", table: "Lessons");
            migrationBuilder.DropIndex(name: "IX_Lessons_GrammarTopicId", table: "Lessons");
            migrationBuilder.DropIndex(name: "IX_Lessons_WritingPromptId", table: "Lessons");
            migrationBuilder.DropColumn(name: "ReadingPassageId", table: "Lessons");
            migrationBuilder.DropColumn(name: "GrammarTopicId", table: "Lessons");
            migrationBuilder.DropColumn(name: "WritingPromptId", table: "Lessons");
            migrationBuilder.DropColumn(name: "ReadingDone", table: "UserLessons");
            migrationBuilder.DropColumn(name: "SpeakingDone", table: "UserLessons");
            migrationBuilder.DropColumn(name: "WritingDone", table: "UserLessons");
            migrationBuilder.DropColumn(name: "GrammarDone", table: "UserLessons");
            migrationBuilder.DropColumn(name: "EaseFactor", table: "UserVocabularies");
            migrationBuilder.DropColumn(name: "IntervalDays", table: "UserVocabularies");
            migrationBuilder.DropColumn(name: "Repetition", table: "UserVocabularies");
            migrationBuilder.DropTable(name: "PlacementItems");
        }
    }
}
