using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Parameter_and_UtilityBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ElectricityNewIndex",
                table: "UtilityBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ElectricityOldIndex",
                table: "UtilityBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WaterNewIndex",
                table: "UtilityBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WaterOldIndex",
                table: "UtilityBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Parameters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Parameters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricityNewIndex",
                table: "UtilityBills");

            migrationBuilder.DropColumn(
                name: "ElectricityOldIndex",
                table: "UtilityBills");

            migrationBuilder.DropColumn(
                name: "WaterNewIndex",
                table: "UtilityBills");

            migrationBuilder.DropColumn(
                name: "WaterOldIndex",
                table: "UtilityBills");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Parameters");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Parameters");
        }
    }
}
