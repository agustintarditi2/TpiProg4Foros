
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
            // Crea la tabla de foros
            migrationBuilder.CreateTable(
                name: "Forums",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "TEXT",
                        nullable: false),

                    Title = table.Column<string>(
                        type: "TEXT",
                        nullable: false),

                    Description = table.Column<string>(
                        type: "TEXT",
                        nullable: false),

                    State = table.Column<bool>(
                        type: "INTEGER",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forums", x => x.Id);
                });

            // Crea un foro general para conservar los posts existentes
            var defaultForumId = new Guid(
                "11111111-1111-1111-1111-111111111111");

            migrationBuilder.InsertData(
                table: "Forums",
                columns: new[]
                {
                    "Id",
                    "Title",
                    "Description",
                    "State"
                },
                values: new object[]
                {
                    defaultForumId,
                    "Foro general",
                    "Foro creado para conservar los posts existentes",
                    true
                });

            // Asigna los posts existentes al foro general
            migrationBuilder.Sql(
                "UPDATE Posts " +
                "SET ForumId = '11111111-1111-1111-1111-111111111111' " +
                "WHERE ForumId = '00000000-0000-0000-0000-000000000000';"
            );

            // Crea el índice de la relación Posts - Forums
            migrationBuilder.CreateIndex(
                name: "IX_Posts_ForumId",
                table: "Posts",
                column: "ForumId");

            // Crea la clave foránea entre Posts y Forums
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
            // Elimina la relación entre Posts y Forums
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Forums_ForumId",
                table: "Posts");

            // Elimina el índice de ForumId
            migrationBuilder.DropIndex(
                name: "IX_Posts_ForumId",
                table: "Posts");

            // Elimina la tabla de foros
            migrationBuilder.DropTable(
                name: "Forums");
        }
    }
}
