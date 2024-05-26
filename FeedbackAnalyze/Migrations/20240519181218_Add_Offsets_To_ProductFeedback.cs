using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackAnalyze.Migrations
{
    /// <inheritdoc />
    public partial class Add_Offsets_To_ProductFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BeginOffset",
                table: "FeedbackSentences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EndOffset",
                table: "FeedbackSentences",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeginOffset",
                table: "FeedbackSentences");

            migrationBuilder.DropColumn(
                name: "EndOffset",
                table: "FeedbackSentences");
        }
    }
}
