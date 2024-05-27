using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackAnalyze.Migrations
{
    /// <inheritdoc />
    public partial class Update_Feedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalText",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalText",
                table: "Feedbacks");
        }
    }
}
