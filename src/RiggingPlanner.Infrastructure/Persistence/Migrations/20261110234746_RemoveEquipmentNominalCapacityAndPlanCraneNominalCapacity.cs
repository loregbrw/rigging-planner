using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEquipmentNominalCapacityAndPlanCraneNominalCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CraneNominalCapacity",
                table: "RiggingPlans");

            migrationBuilder.DropColumn(
                name: "NominalCapacity",
                table: "Equipments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CraneNominalCapacity",
                table: "RiggingPlans",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NominalCapacity",
                table: "Equipments",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: true);
        }
    }
}
