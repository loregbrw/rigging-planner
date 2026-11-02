using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Rigger> Riggers => Set<Rigger>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<EquipmentReport> EquipmentReports => Set<EquipmentReport>();
    public DbSet<Accessory> Accessories => Set<Accessory>();
    public DbSet<AccessoryEvent> AccessoryEvents => Set<AccessoryEvent>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<PlanNumberCounter> PlanNumberCounters => Set<PlanNumberCounter>();
    public DbSet<RiggingPlan> RiggingPlans => Set<RiggingPlan>();
    public DbSet<PlanAccessory> PlanAccessories => Set<PlanAccessory>();
    public DbSet<PlanDrawing> PlanDrawings => Set<PlanDrawing>();
    public DbSet<PlanExport> PlanExports => Set<PlanExport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Cnpj).HasMaxLength(18);
            entity.HasIndex(x => x.Cnpj).IsUnique().HasFilter("Cnpj IS NOT NULL");
        });

        modelBuilder.Entity<Rigger>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RppNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreaNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.EquipmentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TruckModel).HasMaxLength(150).IsRequired();
            entity.Property(x => x.LicensePlate).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ChassisNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CraneBrand).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CraneModel).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NominalCapacity).HasPrecision(18, 3);
            entity.HasIndex(x => x.ChassisNumber).IsUnique();
        });

        modelBuilder.Entity<EquipmentReport>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.ArtNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Equipment).WithMany(x => x.Reports).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.File).WithMany(x => x.EquipmentReports).HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Accessory>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.Name).HasMaxLength(500).IsRequired();
            entity.Property(x => x.UnitWeightKg).HasPrecision(18, 3);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.Tag).WithMany(x => x.Accessories).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.Name).HasMaxLength(100).UseCollation("NOCASE").IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });


        modelBuilder.Entity<AccessoryEvent>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasOne(x => x.Accessory).WithMany(x => x.Events).HasForeignKey(x => x.AccessoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StoredFile>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.OriginalName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.Sha256);
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<PlanNumberCounter>(entity =>
        {
            entity.HasKey(x => x.YearMonth);
            entity.Property(x => x.YearMonth).HasMaxLength(4);
        });

        modelBuilder.Entity<RiggingPlan>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.PlanNumber).HasMaxLength(7).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(300).IsRequired();
            entity.Property(x => x.OperationName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ClientName).HasMaxLength(200);
            entity.Property(x => x.ClientCnpj).HasMaxLength(18);
            entity.Property(x => x.LoadDescription).HasMaxLength(500).IsRequired();
            entity.Property(x => x.TruckModel).HasMaxLength(150);
            entity.Property(x => x.LicensePlate).HasMaxLength(20);
            entity.Property(x => x.ChassisNumber).HasMaxLength(100);
            entity.Property(x => x.CraneBrand).HasMaxLength(100);
            entity.Property(x => x.CraneModel).HasMaxLength(100);
            entity.Property(x => x.QuadrantStart).HasMaxLength(100);
            entity.Property(x => x.QuadrantEnd).HasMaxLength(100);
            entity.Property(x => x.RiggerName).HasMaxLength(200);
            entity.Property(x => x.RiggerRpp).HasMaxLength(50);
            entity.Property(x => x.RiggerCrea).HasMaxLength(50);
            entity.Property(x => x.SupervisorName).HasMaxLength(200);
            entity.Property(x => x.ArtNumber).HasMaxLength(50);
            entity.Property(x => x.Observations).HasMaxLength(4000);
            ConfigurePlanDecimals(entity);
            entity.HasIndex(x => x.PlanNumber).IsUnique();
            entity.HasIndex(x => new { x.Status, x.DeletedAt });
            entity.HasOne(x => x.Folder).WithMany(x => x.Plans).HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Client).WithMany(x => x.Plans).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Equipment).WithMany(x => x.Plans).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Rigger).WithMany(x => x.Plans).HasForeignKey(x => x.RiggerId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ArtFile).WithMany(x => x.PlanArtFiles).HasForeignKey(x => x.ArtFileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EquipmentReport).WithMany(x => x.Plans).HasForeignKey(x => x.EquipmentReportId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlanAccessory>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.Section).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.DrawingRef).HasMaxLength(100);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitWeightKg).HasPrecision(18, 3);
            entity.HasIndex(x => new { x.PlanId, x.SortOrder }).IsUnique();
            entity.HasOne(x => x.Plan).WithMany(x => x.Accessories).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Accessory).WithMany(x => x.PlanItems).HasForeignKey(x => x.AccessoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PlanDrawing>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.HasIndex(x => new { x.PlanId, x.SortOrder }).IsUnique();
            entity.HasOne(x => x.Plan).WithMany(x => x.Drawings).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.File).WithMany(x => x.PlanDrawings).HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlanExport>(entity =>
        {
            ConfigureGuidKey(entity);
            entity.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FileSha256).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.PlanId, x.ExportedAt });
            entity.HasOne(x => x.Plan).WithMany(x => x.Exports).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureGuidKey<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.HasKey("Id");
        entity.Property("Id").ValueGeneratedOnAdd();
    }

    private static void ConfigurePlanDecimals(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RiggingPlan> entity)
    {
        entity.Property(x => x.LengthM).HasPrecision(18, 3);
        entity.Property(x => x.WidthM).HasPrecision(18, 3);
        entity.Property(x => x.HeightM).HasPrecision(18, 3);
        entity.Property(x => x.WeightKg).HasPrecision(18, 3);
        entity.Property(x => x.ExtraLoadKg).HasPrecision(18, 3);
        entity.Property(x => x.CraneNominalCapacity).HasPrecision(18, 3);
        entity.Property(x => x.BoomLengthM).HasPrecision(18, 3);
        entity.Property(x => x.OperatingRadiusM).HasPrecision(18, 3);
        entity.Property(x => x.LoadCapacityT).HasPrecision(18, 3);
        entity.Property(x => x.FlatStrapUtilizationPct).HasPrecision(18, 3);
        entity.Property(x => x.TubularStrapUtilizationPct).HasPrecision(18, 3);
        entity.Property(x => x.WireRopeSlingUtilizationPct).HasPrecision(18, 3);
        entity.Property(x => x.ShackleUtilizationPct).HasPrecision(18, 3);
        entity.Property(x => x.FootingForceKgf).HasPrecision(18, 3);
        entity.Property(x => x.MinGroundResistanceKgfCm2).HasPrecision(18, 3);
        entity.Property(x => x.MaxWindSpeedMs).HasPrecision(18, 3);
    }
}
