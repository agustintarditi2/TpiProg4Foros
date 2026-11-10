using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_RepliedComment",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_RepliedComment",
                table: "Comments",
                column: "RepliedComment",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_RepliedComment",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_RepliedComment",
                table: "Comments",
                column: "RepliedComment",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
