using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AppLearningEnglish.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserVocabulary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLessons_ApplicationUserId",
                table: "UserLessons");

            migrationBuilder.CreateTable(
                name: "UserVocabularies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Familiarity = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    WrongCount = table.Column<int>(type: "integer", nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVocabularies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVocabularies_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVocabularies_words_WordId",
                        column: x => x.WordId,
                        principalTable: "words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_ApplicationUserId_LessonId",
                table: "UserLessons",
                columns: new[] { "ApplicationUserId", "LessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularies_ApplicationUserId_WordId",
                table: "UserVocabularies",
                columns: new[] { "ApplicationUserId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularies_WordId",
                table: "UserVocabularies",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVocabularies");

            migrationBuilder.DropIndex(
                name: "IX_UserLessons_ApplicationUserId_LessonId",
                table: "UserLessons");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_ApplicationUserId",
                table: "UserLessons",
                column: "ApplicationUserId");
        }
    }
}
