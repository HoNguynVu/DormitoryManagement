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
        => optionsBuilder.UseSqlServer("Server=QUANGVINH\\MSSQL;Database=DormitoryDB;User Id=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId);
                                            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.BuildingId);

            entity.HasOne(d => d.Manager).WithMany(p => p.Buildings)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BuildingManager>(entity =>
        {
            entity.HasKey(e => e.ManagerId);

            entity.HasOne(d => d.User).WithMany(p => p.BuildingManagers);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId);

            entity.HasOne(d => d.Room).WithMany(p => p.Contracts);

            entity.HasOne(d => d.Student).WithMany(p => p.Contracts);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId);

            entity.HasOne(d => d.Room).WithMany(p => p.Equipment);
        });

        modelBuilder.Entity<HealthInsurance>(entity =>
        {
            entity.HasKey(e => e.InsuranceId);

            entity.HasOne(d => d.Student).WithMany(p => p.HealthInsurances);
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.OtpId);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.OtpCodes);
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId);
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.PriorityId);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId);

            entity.Property(e => e.PrintTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Student).WithMany(p => p.Receipts);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens);
        });

        modelBuilder.Entity<RegistrationForm>(entity =>
        {
            entity.HasKey(e => e.FormId);

            entity.Property(e => e.RegistrationTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Room).WithMany(p => p.RegistrationForms);

            entity.HasOne(d => d.Student).WithMany(p => p.RegistrationForms);
        });

        modelBuilder.Entity<Relative>(entity =>
        {
            entity.HasKey(e => e.RelativeId);

            entity.HasOne(d => d.Student).WithMany(p => p.Relatives);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);

            entity.HasOne(d => d.Building).WithMany(p => p.Rooms);
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);

            entity.HasOne(d => d.Priority).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UtilityBill>(entity =>
        {
            entity.HasKey(e => e.BillId);

            entity.HasOne(d => d.Room).WithMany(p => p.UtilityBills);
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationId);

            entity.Property(e => e.ViolationTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ReportingManager).WithMany(p => p.Violations)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Student).WithMany(p => p.Violations);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
