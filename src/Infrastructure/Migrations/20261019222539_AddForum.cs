using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForum : Migration
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
                name: "Forums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forums", x => x.Id);
                });

            // crea un foro para conservar los posts existentes
            var defaultForumId = new Guid("11111111-1111-1111-1111-111111111111");

            migrationBuilder.InsertData(
                table: "Forums",
                columns: new[] { "Id", "Title", "Description", "State" },
                values: new object[]
                {
                    defaultForumId,
                    "Foro general",
                    "Foro creado para conservar los posts existentes",
                    true
                });

            // asigna los posts existentes al foro general
            migrationBuilder.Sql(
                "UPDATE Posts " +
                "SET ForumId = '11111111-1111-1111-1111-111111111111' " +
                "WHERE ForumId = '00000000-0000-0000-0000-000000000000';"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ForumId",
                table: "Posts",
                column: "ForumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Forums_ForumId",
                table: "Posts",
                column: "ForumId",
                principalTable: "Forums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Forums_ForumId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "Forums");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ForumId",
                table: "Posts");

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
