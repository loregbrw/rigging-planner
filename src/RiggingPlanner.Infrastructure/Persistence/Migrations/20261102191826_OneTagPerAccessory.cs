using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OneTagPerAccessory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TagId",
                table: "Accessories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Accessories
                SET TagId = (
                    SELECT TagId
                    FROM AccessoryTags
                    WHERE AccessoryTags.AccessoryId = Accessories.Id
                    ORDER BY TagId
                    LIMIT 1
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM AccessoryTags
                    WHERE AccessoryTags.AccessoryId = Accessories.Id
                );
                """);

            migrationBuilder.DropTable(
                name: "AccessoryTags");

            migrationBuilder.CreateIndex(
                name: "IX_Accessories_TagId",
                table: "Accessories",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accessories_Tags_TagId",
                table: "Accessories",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accessories_Tags_TagId",
                table: "Accessories");

            migrationBuilder.DropIndex(
                name: "IX_Accessories_TagId",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "Accessories");

            migrationBuilder.CreateTable(
                name: "AccessoryTags",
                columns: table => new
                {
                    AccessoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessoryTags", x => new { x.AccessoryId, x.TagId });
                    table.ForeignKey(
                        name: "FK_AccessoryTags_Accessories_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessoryTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryTags_TagId",
                table: "AccessoryTags",
                column: "TagId");
        }
    }
}
