using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260912110000_SkillDepthAndReminders")]
    public partial class SkillDepthAndReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ReminderHour",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 19);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "AspNetUsers",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Asia/Ho_Chi_Minh");

            migrationBuilder.CreateTable(
                name: "GrammarTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false),
                    Examples = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReadingPassages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Topic = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingPassages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WritingPrompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    KeyPoints = table.Column<string>(type: "text", nullable: true),
                    SampleAnswer = table.Column<string>(type: "text", nullable: true),
                    MinWords = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingPrompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrammarItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrammarTopicId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    Answer = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarItems_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReadingQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReadingPassageId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    Answer = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingQuestions_ReadingPassages_ReadingPassageId",
                        column: x => x.ReadingPassageId,
                        principalTable: "ReadingPassages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrammarItems_GrammarTopicId",
                table: "GrammarItems",
                column: "GrammarTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingQuestions_ReadingPassageId",
                table: "ReadingQuestions",
                column: "ReadingPassageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GrammarItems");
            migrationBuilder.DropTable(name: "ReadingQuestions");
            migrationBuilder.DropTable(name: "WritingPrompts");
            migrationBuilder.DropTable(name: "GrammarTopics");
            migrationBuilder.DropTable(name: "ReadingPassages");

            migrationBuilder.DropColumn(name: "ReminderEnabled", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ReminderHour", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "TimeZoneId", table: "AspNetUsers");
        }
    }
}
