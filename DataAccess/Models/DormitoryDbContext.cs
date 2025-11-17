using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Entities;

namespace DataAccess.Models;

public partial class DormitoryDbContext : DbContext
{
    public DormitoryDbContext()
    {
    }

    public DormitoryDbContext(DbContextOptions<DormitoryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<BuildingManager> BuildingManagers { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<HealthInsurance> HealthInsurances { get; set; }

    public virtual DbSet<OtpCode> OtpCodes { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<Priority> Priorities { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RegistrationForm> RegistrationForms { get; set; }

    public virtual DbSet<Relative> Relatives { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<UtilityBill> UtilityBills { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS,1433;Database=DormitoryDB;User Id=Admin1;Password=vudz1234;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Accounts__1788CCAC0341C69F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.BuildingId).HasName("PK__Building__5463CDE46D32DD9E");

            entity.HasOne(d => d.Manager).WithMany(p => p.Buildings)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Buildings_BuildingManagers");
        });

        modelBuilder.Entity<BuildingManager>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__Building__3BA2AA8198DD6F12");

            entity.HasIndex(e => e.UserId, "UQ_BuildingManagers_UserID_Filtered")
                .IsUnique()
                .HasFilter("([UserID] IS NOT NULL)");

            entity.HasOne(d => d.User).WithOne(p => p.BuildingManager).HasConstraintName("FK_BuildingManagers_Accounts");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D3409094B649C");

            entity.HasOne(d => d.Room).WithMany(p => p.Contracts).HasConstraintName("FK_Contracts_Rooms");

            entity.HasOne(d => d.Student).WithMany(p => p.Contracts).HasConstraintName("FK_Contracts_Students");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__344745990B4A67BC");

            entity.HasOne(d => d.Room).WithMany(p => p.Equipment).HasConstraintName("FK_Equipment_Rooms");
        });

        modelBuilder.Entity<HealthInsurance>(entity =>
        {
            entity.HasKey(e => e.InsuranceId).HasName("PK__HealthIn__74231BC48C014D6F");

            entity.HasIndex(e => e.StudentId, "UQ_HealthInsurances_StudentID_Filtered")
                .IsUnique()
                .HasFilter("([StudentID] IS NOT NULL)");

            entity.HasOne(d => d.Student).WithOne(p => p.HealthInsurance).HasConstraintName("FK_HealthInsurances_Students");
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__OtpCodes__3143C483D6F517EB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasComputedColumnSql("(CONVERT([bit],case when getdate()<[ExpiresAt] then (1) else (0) end))", false);

            entity.HasOne(d => d.User).WithMany(p => p.OtpCodes).HasConstraintName("FK_OtpCodes_Accounts");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__Paramete__F80C6297FFA99A70");
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.PriorityId).HasName("PK__Prioriti__D0A3D0DECF853390");
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__Receipts__CC08C4003D4B3284");

            entity.Property(e => e.PrintTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Student).WithMany(p => p.Receipts).HasConstraintName("FK_Receipts_Students");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__RefreshT__658FEE8A0AFA753A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasConstraintName("FK_RefreshTokens_Accounts");
        });

        modelBuilder.Entity<RegistrationForm>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__Registra__FB05B7BDE7357B45");

            entity.Property(e => e.RegistrationTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Room).WithMany(p => p.RegistrationForms).HasConstraintName("FK_RegistrationForms_Rooms");

            entity.HasOne(d => d.Student).WithMany(p => p.RegistrationForms).HasConstraintName("FK_RegistrationForms_Students");
        });

        modelBuilder.Entity<Relative>(entity =>
        {
            entity.HasKey(e => e.RelativeId).HasName("PK__Relative__951FE701DA7B54EC");

            entity.HasOne(d => d.Student).WithMany(p => p.Relatives).HasConstraintName("FK_Relatives_Students");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863919077B8A7C");

            entity.HasOne(d => d.Building).WithMany(p => p.Rooms).HasConstraintName("FK_Rooms_Buildings");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4677BB801AAC5");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A79AD81CC97");

            entity.HasIndex(e => e.UserId, "UQ_Students_UserID_Filtered")
                .IsUnique()
                .HasFilter("([UserID] IS NOT NULL)");

            entity.HasOne(d => d.Priority).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Priorities");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Schools");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Accounts");
        });

        modelBuilder.Entity<UtilityBill>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__UtilityB__11F2FC4ADC037E1D");

            entity.HasOne(d => d.Room).WithMany(p => p.UtilityBills).HasConstraintName("FK_UtilityBills_Rooms");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationId).HasName("PK__Violatio__18B6DC28CA2C99C3");

            entity.Property(e => e.ViolationTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ReportingManager).WithMany(p => p.Violations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Violations_BuildingManagers");

            entity.HasOne(d => d.Student).WithMany(p => p.Violations).HasConstraintName("FK_Violations_Students");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
