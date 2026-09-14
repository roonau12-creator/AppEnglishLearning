using System;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260913220000_ProductExperience")]
    public partial class ProductExperience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredTheme",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "light");

            migrationBuilder.AddColumn<string>(
                name: "LearningGoalKind",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "vocab");

            migrationBuilder.AddColumn<int>(
                name: "VocabGoalTotal",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 2000);

            migrationBuilder.AddColumn<decimal>(
                name: "IeltsBandTarget",
                table: "AspNetUsers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Coins",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayerXp",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayerLevel",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "words",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TopicSlug",
                table: "words",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Synonyms",
                table: "words",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Antonyms",
                table: "words",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordFamily",
                table: "words",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "UserVocabularies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHard",
                table: "UserVocabularies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "ReadingPassages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranslationVi",
                table: "ReadingPassages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranslationVi",
                table: "ListeningLessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "ListeningLessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "WritingPrompts",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "paragraph");

            migrationBuilder.CreateTable(
                name: "InAppNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Link = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InAppNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InAppNotifications_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Target = table.Column<int>(type: "integer", nullable: false),
                    RewardCoins = table.Column<int>(type: "integer", nullable: false),
                    RewardXp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    ChallengeId = table.Column<int>(type: "integer", nullable: false),
                    PeriodKey = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChallenges_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChallenges_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumPosts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForumPostId = table.Column<int>(type: "integer", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumComments_ForumPosts_ForumPostId",
                        column: x => x.ForumPostId,
                        principalTable: "ForumPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumComments_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumFollows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerId = table.Column<string>(type: "text", nullable: false),
                    FollowedUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumFollows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumFollows_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumFollows_AspNetUsers_FollowedUserId",
                        column: x => x.FollowedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizMistakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    Expected = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserAnswer = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizMistakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizMistakes_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    GrammarTopicId = table.Column<int>(type: "integer", nullable: false),
                    LastPercent = table.Column<int>(type: "integer", nullable: false),
                    BestPercent = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarProgresses_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrammarProgresses_GrammarTopics_GrammarTopicId",
                        column: x => x.GrammarTopicId,
                        principalTable: "GrammarTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InAppNotifications_ApplicationUserId",
                table: "InAppNotifications",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChallenges_ApplicationUserId_ChallengeId_PeriodKey",
                table: "UserChallenges",
                columns: new[] { "ApplicationUserId", "ChallengeId", "PeriodKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserChallenges_ChallengeId",
                table: "UserChallenges",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_ApplicationUserId",
                table: "ForumPosts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumComments_ForumPostId",
                table: "ForumComments",
                column: "ForumPostId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumComments_ApplicationUserId",
                table: "ForumComments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumFollows_FollowerId_FollowedUserId",
                table: "ForumFollows",
                columns: new[] { "FollowerId", "FollowedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumFollows_FollowedUserId",
                table: "ForumFollows",
                column: "FollowedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizMistakes_ApplicationUserId",
                table: "QuizMistakes",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GrammarProgresses_ApplicationUserId_GrammarTopicId",
                table: "GrammarProgresses",
                columns: new[] { "ApplicationUserId", "GrammarTopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrammarProgresses_GrammarTopicId",
                table: "GrammarProgresses",
                column: "GrammarTopicId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GrammarProgresses");
            migrationBuilder.DropTable(name: "QuizMistakes");
            migrationBuilder.DropTable(name: "ForumFollows");
            migrationBuilder.DropTable(name: "ForumComments");
            migrationBuilder.DropTable(name: "ForumPosts");
            migrationBuilder.DropTable(name: "UserChallenges");
            migrationBuilder.DropTable(name: "Challenges");
            migrationBuilder.DropTable(name: "InAppNotifications");
            migrationBuilder.DropColumn(name: "PreferredTheme", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LearningGoalKind", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "VocabGoalTotal", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "IeltsBandTarget", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "Coins", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PlayerXp", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PlayerLevel", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "Level", table: "words");
            migrationBuilder.DropColumn(name: "TopicSlug", table: "words");
            migrationBuilder.DropColumn(name: "Synonyms", table: "words");
            migrationBuilder.DropColumn(name: "Antonyms", table: "words");
            migrationBuilder.DropColumn(name: "WordFamily", table: "words");
            migrationBuilder.DropColumn(name: "IsFavorite", table: "UserVocabularies");
            migrationBuilder.DropColumn(name: "IsHard", table: "UserVocabularies");
            migrationBuilder.DropColumn(name: "AudioUrl", table: "ReadingPassages");
            migrationBuilder.DropColumn(name: "TranslationVi", table: "ReadingPassages");
            migrationBuilder.DropColumn(name: "TranslationVi", table: "ListeningLessons");
            migrationBuilder.DropColumn(name: "VideoUrl", table: "ListeningLessons");
            migrationBuilder.DropColumn(name: "Genre", table: "WritingPrompts");
        }
    }
}
