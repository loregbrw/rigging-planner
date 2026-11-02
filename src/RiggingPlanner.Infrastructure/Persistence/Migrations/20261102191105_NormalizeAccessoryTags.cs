using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAccessoryTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.Sql("""
                WITH RECURSIVE split(accessory_id, rest, tag) AS (
                    SELECT Id, COALESCE(Tags, '') || ',', ''
                    FROM Accessories
                    WHERE Tags IS NOT NULL AND trim(Tags) <> ''
                    UNION ALL
                    SELECT accessory_id,
                           substr(rest, instr(rest, ',') + 1),
                           trim(substr(rest, 1, instr(rest, ',') - 1))
                    FROM split
                    WHERE instr(rest, ',') > 0
                )
                INSERT OR IGNORE INTO Tags (Id, Name, CreatedAt)
                SELECT printf('%s-%s-%s-%s-%s',
                              lower(hex(randomblob(4))),
                              lower(hex(randomblob(2))),
                              lower(hex(randomblob(2))),
                              lower(hex(randomblob(2))),
                              lower(hex(randomblob(6)))),
                       tag,
                       datetime('now')
                FROM split
                WHERE trim(tag) <> '';

                WITH RECURSIVE split(accessory_id, rest, tag) AS (
                    SELECT Id, COALESCE(Tags, '') || ',', ''
                    FROM Accessories
                    WHERE Tags IS NOT NULL AND trim(Tags) <> ''
                    UNION ALL
                    SELECT accessory_id,
                           substr(rest, instr(rest, ',') + 1),
                           trim(substr(rest, 1, instr(rest, ',') - 1))
                    FROM split
                    WHERE instr(rest, ',') > 0
                )
                INSERT OR IGNORE INTO AccessoryTags (AccessoryId, TagId)
                SELECT split.accessory_id, Tags.Id
                FROM split
                INNER JOIN Tags ON Tags.Name = split.tag
                WHERE trim(split.tag) <> '';
                """);

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Accessories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessoryTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Accessories",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
