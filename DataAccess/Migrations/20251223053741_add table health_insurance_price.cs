using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addtablehealth_insurance_price : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HealthPriceID",
                table: "HealthInsurances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "HealthInsurancePrices",
                columns: table => new
                {
                    HealthPriceID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthInsurancePrices", x => x.HealthPriceID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthInsurances_HealthPriceID",
                table: "HealthInsurances",
                column: "HealthPriceID");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthInsurances_HealthInsurancePrices_HealthPriceID",
                table: "HealthInsurances",
                column: "HealthPriceID",
                principalTable: "HealthInsurancePrices",
                principalColumn: "HealthPriceID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthInsurances_HealthInsurancePrices_HealthPriceID",
                table: "HealthInsurances");

            migrationBuilder.DropTable(
                name: "HealthInsurancePrices");

            migrationBuilder.DropIndex(
                name: "IX_HealthInsurances_HealthPriceID",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "HealthPriceID",
                table: "HealthInsurances");
        }
    }
}
