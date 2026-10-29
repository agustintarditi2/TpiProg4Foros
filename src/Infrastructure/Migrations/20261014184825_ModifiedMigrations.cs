using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Edited",
                table: "Posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ForumId",
                table: "Posts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "Posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DateEdited",
                table: "Comments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "Edited",
                table: "Comments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "Comments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Post = table.Column<Guid>(type: "TEXT", nullable: true),
                    Comment = table.Column<Guid>(type: "TEXT", nullable: true),
                    User = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_Reactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Poster",
                table: "Comments",
                column: "Poster");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_User_Post_Comment",
                table: "Reactions",
                columns: new[] { "User", "Post", "Comment" });

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_Poster",
                table: "Comments",
                column: "Poster",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_Poster",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_Poster",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Edited",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ForumId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Edited",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "Comments");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DateEdited",
                table: "Comments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
