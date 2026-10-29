using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiggingPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accessories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LengthM = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    CapacityT = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    UnitWeightKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    DrawingRef = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ReviewDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accessories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TruckModel = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ChassisNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ManufactureYear = table.Column<int>(type: "INTEGER", nullable: true),
                    CraneBrand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CraneModel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NominalCapacity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanNumberCounters",
                columns: table => new
                {
                    YearMonth = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    LastSequence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanNumberCounters", x => x.YearMonth);
                });

            migrationBuilder.CreateTable(
                name: "Riggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    RppNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreaNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "EquipmentReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentReports_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentReports_StoredFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiggingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlanNumber = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    FolderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlanDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    OperationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ClientCnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: true),
                    LoadDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LengthM = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    WidthM = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    HeightM = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    WeightKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    ExtraLoadKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TruckModel = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ChassisNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ManufactureYear = table.Column<int>(type: "INTEGER", nullable: true),
                    CraneBrand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CraneModel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CraneNominalCapacity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    BoomLengthM = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    OperatingRadiusM = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    LoadCapacityT = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    QuadrantStart = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    QuadrantEnd = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FlatStrapUtilizationPct = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    TubularStrapUtilizationPct = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    WireRopeSlingUtilizationPct = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    ShackleUtilizationPct = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    FootingForceKgf = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    MinGroundResistanceKgfCm2 = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    MaxWindSpeedMs = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: true),
                    RiggerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RiggerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RiggerRpp = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SupervisorName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ArtNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ArtFileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EquipmentReportId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Observations = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiggingPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiggingPlans_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RiggingPlans_EquipmentReports_EquipmentReportId",
                        column: x => x.EquipmentReportId,
                        principalTable: "EquipmentReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiggingPlans_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RiggingPlans_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RiggingPlans_Riggers_RiggerId",
                        column: x => x.RiggerId,
                        principalTable: "Riggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RiggingPlans_StoredFiles_ArtFileId",
                        column: x => x.ArtFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanAccessories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Section = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    AccessoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DrawingRef = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitWeightKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanAccessories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanAccessories_Accessories_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlanAccessories_RiggingPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "RiggingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanDrawings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDrawings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanDrawings_RiggingPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "RiggingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanDrawings_StoredFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    WasFinal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanExports_RiggingPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "RiggingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accessories_Status",
                table: "Accessories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryEvents_AccessoryId",
                table: "AccessoryEvents",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Cnpj",
                table: "Clients",
                column: "Cnpj",
                unique: true,
                filter: "Cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentReports_EquipmentId",
                table: "EquipmentReports",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentReports_FileId",
                table: "EquipmentReports",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_ChassisNumber",
                table: "Equipments",
                column: "ChassisNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_Name",
                table: "Folders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanAccessories_AccessoryId",
                table: "PlanAccessories",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanAccessories_PlanId_SortOrder",
                table: "PlanAccessories",
                columns: new[] { "PlanId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanDrawings_FileId",
                table: "PlanDrawings",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDrawings_PlanId_SortOrder",
                table: "PlanDrawings",
                columns: new[] { "PlanId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanExports_PlanId_ExportedAt",
                table: "PlanExports",
                columns: new[] { "PlanId", "ExportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_ArtFileId",
                table: "RiggingPlans",
                column: "ArtFileId");

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_ClientId",
                table: "RiggingPlans",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_EquipmentId",
                table: "RiggingPlans",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_EquipmentReportId",
                table: "RiggingPlans",
                column: "EquipmentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_FolderId",
                table: "RiggingPlans",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_PlanNumber",
                table: "RiggingPlans",
                column: "PlanNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_RiggerId",
                table: "RiggingPlans",
                column: "RiggerId");

            migrationBuilder.CreateIndex(
                name: "IX_RiggingPlans_Status_DeletedAt",
                table: "RiggingPlans",
                columns: new[] { "Status", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Sha256",
                table: "StoredFiles",
                column: "Sha256");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessoryEvents");

            migrationBuilder.DropTable(
                name: "PlanAccessories");

            migrationBuilder.DropTable(
                name: "PlanDrawings");

            migrationBuilder.DropTable(
                name: "PlanExports");

            migrationBuilder.DropTable(
                name: "PlanNumberCounters");

            migrationBuilder.DropTable(
                name: "Accessories");

            migrationBuilder.DropTable(
                name: "RiggingPlans");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "EquipmentReports");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Riggers");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "StoredFiles");
        }
    }
}
