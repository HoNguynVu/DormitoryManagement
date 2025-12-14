using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentinMaintenanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EquipmentID",
                table: "MaintenanceRequests",
                type: "nvarchar(128)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_EquipmentID",
                table: "MaintenanceRequests",
                column: "EquipmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Equipment_EquipmentID",
                table: "MaintenanceRequests",
                column: "EquipmentID",
                principalTable: "Equipment",
                principalColumn: "EquipmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Equipment_EquipmentID",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_EquipmentID",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "EquipmentID",
                table: "MaintenanceRequests");
        }
    }
}
