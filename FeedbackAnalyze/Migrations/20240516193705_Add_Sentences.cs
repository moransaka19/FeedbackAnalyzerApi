using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FeedbackAnalyze.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sentences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_ProductFeedbacks_ProductFeedbackId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ProductFeedbackId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "ProductFeedbackId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Feedbacks");

            migrationBuilder.CreateTable(
                name: "FeedbackSentences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false),
                    ProductFeedbackId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSentences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackSentences_ProductFeedbacks_ProductFeedbackId",
                        column: x => x.ProductFeedbackId,
                        principalTable: "ProductFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackFeedbackSentence",
                columns: table => new
                {
                    FeedbackSentencesId = table.Column<int>(type: "integer", nullable: false),
                    FeedbacksId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackFeedbackSentence", x => new { x.FeedbackSentencesId, x.FeedbacksId });
                    table.ForeignKey(
                        name: "FK_FeedbackFeedbackSentence_FeedbackSentences_FeedbackSentence~",
                        column: x => x.FeedbackSentencesId,
                        principalTable: "FeedbackSentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackFeedbackSentence_Feedbacks_FeedbacksId",
                        column: x => x.FeedbacksId,
                        principalTable: "Feedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackFeedbackSentence_FeedbacksId",
                table: "FeedbackFeedbackSentence",
                column: "FeedbacksId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSentences_ProductFeedbackId",
                table: "FeedbackSentences",
                column: "ProductFeedbackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackFeedbackSentence");

            migrationBuilder.DropTable(
                name: "FeedbackSentences");

            migrationBuilder.AddColumn<int>(
                name: "ProductFeedbackId",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ProductFeedbackId",
                table: "Feedbacks",
                column: "ProductFeedbackId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_ProductFeedbacks_ProductFeedbackId",
                table: "Feedbacks",
                column: "ProductFeedbackId",
                principalTable: "ProductFeedbacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
