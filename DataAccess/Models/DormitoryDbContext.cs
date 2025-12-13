using System;
using System.Collections.Generic;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models
{
    public partial class DormitoryDbContext : DbContext
    {
        public DormitoryDbContext()
        {
        }

        public DormitoryDbContext(DbContextOptions<DormitoryDbContext> options)
            : base(options)
        {
        }

        // Auth
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<OtpCode> OtpCodes { get; set; }

        // Person Inheritance Group
        // Không cần DbSet<Person> vì đây là abstract class
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Relative> Relatives { get; set; }
        public virtual DbSet<BuildingManager> BuildingManagers { get; set; }

        // Master Data
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<RoomType> RoomTypes { get; set; }

        // Infrastructure
        public virtual DbSet<Building> Buildings { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Equipment> Equipment { get; set; }

        // Operations
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<RegistrationForm> RegistrationForms { get; set; }
        public virtual DbSet<Violation> Violations { get; set; }
        public virtual DbSet<HealthInsurance> HealthInsurances { get; set; }

        // Finance
        public virtual DbSet<UtilityBill> UtilityBills { get; set; }
        public virtual DbSet<Receipt> Receipts { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }

        public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Lưu ý: Nên chuyển connection string vào appsettings.json
                optionsBuilder.UseSqlServer("Server=(local);Database=DormitoryDB;uid=sa;Password=123456;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Cấu hình Inheritance cho Person ---
            // EF Core sẽ tự động map các property của Person xuống các bảng con
            // vì Person là abstract và không có DbSet<Person>.

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(e => e.StudentID);
                
                // Map property Address của Person vào cột CurrentAddress trong DB
                entity.Property(e => e.Address).HasColumnName("CurrentAddress");
                
                // Cấu hình quan hệ
                entity.HasOne(d => d.Account).WithMany().HasForeignKey(d => d.AccountID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.School).WithMany(p => p.Students).HasForeignKey(d => d.SchoolID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Priority).WithMany(p => p.Students).HasForeignKey(d => d.PriorityID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Relative>(entity =>
            {
                entity.ToTable("Relatives");
                entity.HasKey(e => e.RelativeID);
                
                // Address của Relative map vào cột Address mặc định nên không cần config thêm
                
                entity.HasOne(d => d.Student).WithMany(p => p.Relatives).HasForeignKey(d => d.StudentID).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BuildingManager>(entity =>
            {
                entity.ToTable("BuildingManagers");
                entity.HasKey(e => e.ManagerID);
                
                entity.HasOne(d => d.Account).WithMany().HasForeignKey(d => d.AccountID).OnDelete(DeleteBehavior.Cascade);
            });

            // --- Các cấu hình khác ---

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.BuildingID);
                entity.HasIndex(e => e.BuildingName).IsUnique();
                entity.HasOne(d => d.Manager).WithMany(p => p.Buildings).HasForeignKey(d => d.ManagerID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomID);
                entity.HasIndex(e => e.RoomName).IsUnique();
                entity.HasOne(d => d.Building).WithMany(p => p.Rooms).HasForeignKey(d => d.BuildingID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.RoomType).WithMany(p => p.Rooms).HasForeignKey(d => d.RoomTypeID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasKey(e => e.RoomTypeID);
                entity.HasIndex(e => e.TypeName).IsUnique();
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.ContractID);
                entity.HasOne(d => d.Room).WithMany(p => p.Contracts).HasForeignKey(d => d.RoomID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Student).WithMany(p => p.Contracts).HasForeignKey(d => d.StudentID).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<RegistrationForm>(entity =>
            {
                entity.HasKey(e => e.FormID);
                entity.Property(e => e.RegistrationTime).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Room).WithMany(p => p.RegistrationForms).HasForeignKey(d => d.RoomID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Student).WithMany(p => p.RegistrationForms).HasForeignKey(d => d.StudentID).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Violation>(entity =>
            {
                entity.HasKey(e => e.ViolationID);
                entity.Property(e => e.ViolationTime).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.ReportingManager).WithMany(p => p.ReportedViolations).HasForeignKey(d => d.ReportingManagerID).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Student).WithMany(p => p.Violations).HasForeignKey(d => d.StudentID).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<UtilityBill>(entity =>
            {
                entity.HasKey(e => e.BillID);
                entity.HasOne(d => d.Room).WithMany(p => p.UtilityBills).HasForeignKey(d => d.RoomID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasKey(e => e.ReceiptID);
                entity.Property(e => e.PrintTime).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Student).WithMany(p => p.Receipts).HasForeignKey(d => d.StudentID).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentID);
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.EquipmentID);
                entity.HasOne(d => d.Room).WithMany(p => p.Equipment).HasForeignKey(d => d.RoomID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.TokenID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Account).WithMany().HasForeignKey(d => d.AccountID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(e => e.OtpID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(d => d.Account).WithMany().HasForeignKey(d => d.AccountID).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<HealthInsurance>(entity =>
            {
                entity.HasKey(e => e.InsuranceID);
                entity.HasOne(d => d.Student).WithMany(p => p.HealthInsurances).HasConstraintName("FK_HealthInsurances_Students");
            });

            modelBuilder.Entity<MaintenanceRequest>(entity =>
            {
                entity.HasKey(e => e.RequestID);
                entity.Property(e => e.RequestDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Room)
                      .WithMany(p => p.MaintenanceRequests) 
                      .HasForeignKey(d => d.RoomID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Student)
                      .WithMany() 
                      .HasForeignKey(d => d.StudentID)
                      .OnDelete(DeleteBehavior.NoAction); 

                entity.HasOne(d => d.Equipment)
                      .WithMany() 
                      .HasForeignKey(d => d.EquipmentID)
                      .OnDelete(DeleteBehavior.SetNull);    
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
