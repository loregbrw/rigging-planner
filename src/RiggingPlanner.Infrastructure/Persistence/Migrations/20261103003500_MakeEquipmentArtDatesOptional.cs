using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations;

[Migration("20261103003500_MakeEquipmentArtDatesOptional")]
public partial class MakeEquipmentArtDatesOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "ValidTo",
            table: "EquipmentReports",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<DateTime>(
            name: "ValidFrom",
            table: "EquipmentReports",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "ValidTo",
            table: "EquipmentReports",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ValidFrom",
            table: "EquipmentReports",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true);
    }
}
