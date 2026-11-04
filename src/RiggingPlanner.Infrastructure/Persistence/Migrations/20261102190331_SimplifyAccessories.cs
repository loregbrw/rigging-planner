using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyAccessories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Accessories",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Accessories",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Accessories
                SET Tags = Category
                WHERE Category IS NOT NULL AND trim(Category) <> '';
                """);

            migrationBuilder.DropColumn(
                name: "CapacityT",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "DrawingRef",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "LengthM",
                table: "Accessories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Accessories");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Accessories",
                newName: "Description");

            migrationBuilder.AddColumn<decimal>(
                name: "CapacityT",
                table: "Accessories",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Accessories",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DrawingRef",
                table: "Accessories",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthM",
                table: "Accessories",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: true);
        }
    }
}
