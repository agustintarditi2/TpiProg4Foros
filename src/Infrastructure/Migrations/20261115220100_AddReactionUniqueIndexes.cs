using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddReactionUniqueIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Reactions_User_Post_Comment", table: "Reactions");
            migrationBuilder.CreateIndex(
                name: "IX_Reactions_User_Comment",
                table: "Reactions",
                columns: new[] { "User", "Comment" },
                unique: true,
                filter: "\"Comment\" IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "IX_Reactions_User_Post",
                table: "Reactions",
                columns: new[] { "User", "Post" },
                unique: true,
                filter: "\"Post\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Reactions_User_Comment", table: "Reactions");
            migrationBuilder.DropIndex(name: "IX_Reactions_User_Post", table: "Reactions");
            migrationBuilder.CreateIndex(
                name: "IX_Reactions_User_Post_Comment",
                table: "Reactions",
                columns: new[] { "User", "Post", "Comment" });
        }
    }
}
