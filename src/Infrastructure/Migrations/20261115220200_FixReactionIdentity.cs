using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class FixReactionIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // conserva los datos y asigna nuevos ids globales autoincrementales
            migrationBuilder.Sql("""
                CREATE TABLE "__Reactions_new" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Reactions" PRIMARY KEY AUTOINCREMENT,
                    "UserId" TEXT NOT NULL,
                    "Comment" TEXT NULL,
                    "Post" TEXT NULL,
                    "Type" INTEGER NOT NULL,
                    "User" TEXT NOT NULL,
                    CONSTRAINT "FK_Reactions_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );

                INSERT INTO "__Reactions_new" ("UserId", "Comment", "Post", "Type", "User")
                    SELECT "UserId", "Comment", "Post", "Type", "User"
                    FROM "Reactions";

                DROP TABLE "Reactions";
                ALTER TABLE "__Reactions_new" RENAME TO "Reactions";
                CREATE INDEX "IX_Reactions_UserId" ON "Reactions" ("UserId");
                CREATE UNIQUE INDEX "IX_Reactions_User_Comment"
                    ON "Reactions" ("User", "Comment") WHERE "Comment" IS NOT NULL;
                CREATE UNIQUE INDEX "IX_Reactions_User_Post"
                    ON "Reactions" ("User", "Post") WHERE "Post" IS NOT NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restaura la clave compuesta sin descartar reacciones
            migrationBuilder.Sql("""
                CREATE TABLE "__Reactions_old" (
                    "UserId" TEXT NOT NULL,
                    "Id" INTEGER NOT NULL,
                    "Comment" TEXT NULL,
                    "Post" TEXT NULL,
                    "Type" INTEGER NOT NULL,
                    "User" TEXT NOT NULL,
                    CONSTRAINT "PK_Reactions" PRIMARY KEY ("UserId", "Id"),
                    CONSTRAINT "FK_Reactions_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );

                INSERT INTO "__Reactions_old" ("UserId", "Id", "Comment", "Post", "Type", "User")
                    SELECT "UserId", "Id", "Comment", "Post", "Type", "User"
                    FROM "Reactions";

                DROP TABLE "Reactions";
                ALTER TABLE "__Reactions_old" RENAME TO "Reactions";
                CREATE UNIQUE INDEX "IX_Reactions_User_Comment"
                    ON "Reactions" ("User", "Comment") WHERE "Comment" IS NOT NULL;
                CREATE UNIQUE INDEX "IX_Reactions_User_Post"
                    ON "Reactions" ("User", "Post") WHERE "Post" IS NOT NULL;
                """);
        }
    }
}
