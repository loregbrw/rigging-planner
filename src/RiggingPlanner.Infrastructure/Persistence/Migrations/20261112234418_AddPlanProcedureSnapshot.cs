using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanProcedureSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanProcedureSnapshots",
                columns: table => new
                {
                    PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CraneModelCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ContentSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanProcedureSnapshots", x => x.PlanId);
                    table.ForeignKey(
                        name: "FK_PlanProcedureSnapshots_RiggingPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "RiggingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanProcedureSnapshots");
        }
    }
}
