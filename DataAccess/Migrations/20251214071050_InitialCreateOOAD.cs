using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateOOAD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    ParameterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultElectricityPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultWaterPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.ParameterID);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ReceiptID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    TransactionID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                });

            migrationBuilder.CreateTable(
                name: "Priorities",
                columns: table => new
                {
                    PriorityID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PriorityDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priorities", x => x.PriorityID);
                });

            migrationBuilder.CreateTable(
                name: "RoomTypes",
                columns: table => new
                {
                    RoomTypeID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypes", x => x.RoomTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    SchoolID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.SchoolID);
                });

            migrationBuilder.CreateTable(
                name: "BuildingManagers",
                columns: table => new
                {
                    ManagerID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AccountID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CitizenID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccountUserId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingManagers", x => x.ManagerID);
                    table.ForeignKey(
                        name: "FK_BuildingManagers_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingManagers_Accounts_AccountUserId",
                        column: x => x.AccountUserId,
                        principalTable: "Accounts",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    OtpID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AccountID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AccountUserId = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCodes", x => x.OtpID);
                    table.ForeignKey(
                        name: "FK_OtpCodes_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OtpCodes_Accounts_AccountUserId",
                        column: x => x.AccountUserId,
                        principalTable: "Accounts",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    TokenID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AccountID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountUserId = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.TokenID);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Accounts_AccountUserId",
                        column: x => x.AccountUserId,
                        principalTable: "Accounts",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AccountID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CitizenID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CitizenIDIssuePlace = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CurrentAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SchoolID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PriorityID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AccountUserId = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_Students_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_Accounts_AccountUserId",
                        column: x => x.AccountUserId,
                        principalTable: "Accounts",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Students_Priorities_PriorityID",
                        column: x => x.PriorityID,
                        principalTable: "Priorities",
                        principalColumn: "PriorityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    BuildingID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BuildingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManagerID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.BuildingID);
                    table.ForeignKey(
                        name: "FK_Buildings_BuildingManagers_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "BuildingManagers",
                        principalColumn: "ManagerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthInsurances",
                columns: table => new
                {
                    InsuranceID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InitialHospital = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthInsurances", x => x.InsuranceID);
                    table.ForeignKey(
                        name: "FK_HealthInsurances_Students",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    ReceiptID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PrintTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RelatedObjectID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.ReceiptID);
                    table.ForeignKey(
                        name: "FK_Receipts_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateTable(
                name: "Relatives",
                columns: table => new
                {
                    RelativeID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relatives", x => x.RelativeID);
                    table.ForeignKey(
                        name: "FK_Relatives_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateTable(
                name: "Violations",
                columns: table => new
                {
                    ViolationID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ReportingManagerID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ViolationAct = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ViolationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violations", x => x.ViolationID);
                    table.ForeignKey(
                        name: "FK_Violations_BuildingManagers_ReportingManagerID",
                        column: x => x.ReportingManagerID,
                        principalTable: "BuildingManagers",
                        principalColumn: "ManagerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Violations_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BuildingID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoomTypeID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RoomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CurrentOccupancy = table.Column<int>(type: "int", nullable: false),
                    RoomStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsUnderMaintenance = table.Column<bool>(type: "bit", nullable: false),
                    IsBeingCleaned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomID);
                    table.ForeignKey(
                        name: "FK_Rooms_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "BuildingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rooms_RoomTypes_RoomTypeID",
                        column: x => x.RoomTypeID,
                        principalTable: "RoomTypes",
                        principalColumn: "RoomTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ContractStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractID);
                    table.ForeignKey(
                        name: "FK_Contracts_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contracts_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EquipmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentID);
                    table.ForeignKey(
                        name: "FK_Equipment_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationForms",
                columns: table => new
                {
                    FormID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RegistrationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationForms", x => x.FormID);
                    table.ForeignKey(
                        name: "FK_RegistrationForms_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationForms_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateTable(
                name: "UtilityBills",
                columns: table => new
                {
                    BillID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ElectricityOldIndex = table.Column<int>(type: "int", nullable: false),
                    ElectricityNewIndex = table.Column<int>(type: "int", nullable: false),
                    WaterOldIndex = table.Column<int>(type: "int", nullable: false),
                    WaterNewIndex = table.Column<int>(type: "int", nullable: false),
                    ElectricityUsage = table.Column<int>(type: "int", nullable: false),
                    WaterUsage = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityBills", x => x.BillID);
                    table.ForeignKey(
                        name: "FK_UtilityBills_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRequests",
                columns: table => new
                {
                    RequestID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentID = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    EquipmentID = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RepairCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManagerNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID");
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Username",
                table: "Accounts",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Accounts__536C85E491C65809",
                table: "Accounts",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Accounts__A9D10534E8EB8D7E",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingManagers_AccountID",
                table: "BuildingManagers",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingManagers_AccountUserId",
                table: "BuildingManagers",
                column: "AccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_BuildingName",
                table: "Buildings",
                column: "BuildingName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ManagerID",
                table: "Buildings",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_RoomID",
                table: "Contracts",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_StudentID",
                table: "Contracts",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_RoomID",
                table: "Equipment",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_HealthInsurances_StudentID",
                table: "HealthInsurances",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_EquipmentID",
                table: "MaintenanceRequests",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_RoomID",
                table: "MaintenanceRequests",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_StudentID",
                table: "MaintenanceRequests",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AccountID",
                table: "Notifications",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_AccountID",
                table: "OtpCodes",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_AccountUserId",
                table: "OtpCodes",
                column: "AccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_StudentID",
                table: "Receipts",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AccountID",
                table: "RefreshTokens",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AccountUserId",
                table: "RefreshTokens",
                column: "AccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationForms_RoomID",
                table: "RegistrationForms",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationForms_StudentID",
                table: "RegistrationForms",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Relatives_StudentID",
                table: "Relatives",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BuildingID",
                table: "Rooms",
                column: "BuildingID");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomName",
                table: "Rooms",
                column: "RoomName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomTypeID",
                table: "Rooms",
                column: "RoomTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypes_TypeName",
                table: "RoomTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_AccountID",
                table: "Students",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AccountUserId",
                table: "Students",
                column: "AccountUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_PriorityID",
                table: "Students",
                column: "PriorityID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolID",
                table: "Students",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityBills_RoomID",
                table: "UtilityBills",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_ReportingManagerID",
                table: "Violations",
                column: "ReportingManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_StudentID",
                table: "Violations",
                column: "StudentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "HealthInsurances");

            migrationBuilder.DropTable(
                name: "MaintenanceRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RegistrationForms");

            migrationBuilder.DropTable(
                name: "Relatives");

            migrationBuilder.DropTable(
                name: "UtilityBills");

            migrationBuilder.DropTable(
                name: "Violations");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Priorities");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "RoomTypes");

            migrationBuilder.DropTable(
                name: "BuildingManagers");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
