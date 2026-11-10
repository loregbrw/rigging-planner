using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGlobalAccessoriesAndAccessoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanAccessories_Accessories_AccessoryId",
                table: "PlanAccessories");

            migrationBuilder.DropTable(
                name: "AccessoryEvents");

            migrationBuilder.DropTable(
                name: "Accessories");

            migrationBuilder.DropIndex(
                name: "IX_PlanAccessories_AccessoryId",
                table: "PlanAccessories");

            migrationBuilder.DropColumn(
                name: "AccessoryId",
                table: "PlanAccessories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccessoryId",
                table: "PlanAccessories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accessories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ReviewDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    UnitWeightKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accessories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accessories_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AccessoryEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessoryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessoryEvents_Accessories_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanAccessories_AccessoryId",
                table: "PlanAccessories",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Accessories_Status",
                table: "Accessories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Accessories_TagId",
                table: "Accessories",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryEvents_AccessoryId",
                table: "AccessoryEvents",
                column: "AccessoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanAccessories_Accessories_AccessoryId",
                table: "PlanAccessories",
                column: "AccessoryId",
                principalTable: "Accessories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
