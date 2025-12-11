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

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Priority> Priorities { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RegistrationForm> RegistrationForms { get; set; }

    public virtual DbSet<Relative> Relatives { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomType> RoomTypes { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<UtilityBill> UtilityBills { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Database=DormitoryDB;uid=sa;pwd=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Accounts__1788CCACD4B1B5FB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.BuildingId).HasName("PK__Building__5463CDE472F586C4");

            entity.HasOne(d => d.Manager).WithMany(p => p.Buildings)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Buildings_BuildingManagers");
        });

        modelBuilder.Entity<BuildingManager>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__Building__3BA2AA81D266DD72");

            entity.HasOne(d => d.User).WithMany(p => p.BuildingManagers).HasConstraintName("FK_BuildingManagers_Accounts");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D34096A9346E3");

            entity.HasOne(d => d.Room).WithMany(p => p.Contracts).HasConstraintName("FK_Contracts_Rooms");

            entity.HasOne(d => d.Student).WithMany(p => p.Contracts).HasConstraintName("FK_Contracts_Students");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__34474599D1266794");

            entity.HasOne(d => d.Room).WithMany(p => p.Equipment).HasConstraintName("FK_Equipment_Rooms");
        });

        modelBuilder.Entity<HealthInsurance>(entity =>
        {
            entity.HasKey(e => e.InsuranceId).HasName("PK__HealthIn__74231BC451D8F661");

            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.Student).WithMany(p => p.HealthInsurances).HasConstraintName("FK_HealthInsurances_Students");
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__OtpCodes__3143C483E2152EE4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.OtpCodes).HasConstraintName("FK_OtpCodes_Accounts");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__Paramete__F80C6297233AACC1");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58DC5D0813");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.PriorityId).HasName("PK__Prioriti__D0A3D0DE98737F0F");
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__Receipts__CC08C400E8AEE7E0");

            entity.Property(e => e.PrintTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Student).WithMany(p => p.Receipts).HasConstraintName("FK_Receipts_Students");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__RefreshT__658FEE8A3880FBFF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasConstraintName("FK_RefreshTokens_Accounts");
        });

        modelBuilder.Entity<RegistrationForm>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__Registra__FB05B7BD3CFCF568");

            entity.Property(e => e.RegistrationTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Room).WithMany(p => p.RegistrationForms).HasConstraintName("FK_RegistrationForms_Rooms");

            entity.HasOne(d => d.Student).WithMany(p => p.RegistrationForms).HasConstraintName("FK_RegistrationForms_Students");
        });

        modelBuilder.Entity<Relative>(entity =>
        {
            entity.HasKey(e => e.RelativeId).HasName("PK__Relative__951FE701972CA852");

            entity.HasOne(d => d.Student).WithMany(p => p.Relatives).HasConstraintName("FK_Relatives_Students");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863919654F63D0");

            entity.HasOne(d => d.Building).WithMany(p => p.Rooms).HasConstraintName("FK_Rooms_Buildings");

            entity.HasOne(d => d.RoomType).WithMany(p => p.Rooms)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Rooms_RoomTypes");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId).HasName("PK__RoomType__BCC89611CD2B7C4F");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4677B90325D29");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A792BE5B323");

            entity.HasOne(d => d.Priority).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Priorities");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Schools");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Accounts");
        });

        modelBuilder.Entity<UtilityBill>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__UtilityB__11F2FC4A5E476280");

            entity.HasOne(d => d.Room).WithMany(p => p.UtilityBills).HasConstraintName("FK_UtilityBills_Rooms");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationId).HasName("PK__Violatio__18B6DC28D917E042");

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
