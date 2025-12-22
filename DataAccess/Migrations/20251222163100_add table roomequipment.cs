using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addtableroomequipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_Rooms_RoomID",
                table: "Equipment");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_RoomID",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "RoomID",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Equipment");

            migrationBuilder.CreateTable(
                name: "RoomEquipments",
                columns: table => new
                {
                    RoomEquipmentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoomID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EquipmentID = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EquipmentID1 = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomEquipments", x => x.RoomEquipmentID);
                    table.ForeignKey(
                        name: "FK_RoomEquipments_Equipment_EquipmentID",
                        column: x => x.EquipmentID,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomEquipments_Equipment_EquipmentID1",
                        column: x => x.EquipmentID1,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentID");
                    table.ForeignKey(
                        name: "FK_RoomEquipments_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipments_EquipmentID",
                table: "RoomEquipments",
                column: "EquipmentID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipments_EquipmentID1",
                table: "RoomEquipments",
                column: "EquipmentID1");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipments_RoomID",
                table: "RoomEquipments",
                column: "RoomID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomEquipments");

            migrationBuilder.AddColumn<string>(
                name: "RoomID",
                table: "Equipment",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Equipment",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_RoomID",
                table: "Equipment",
                column: "RoomID");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_Rooms_RoomID",
                table: "Equipment",
                column: "RoomID",
                principalTable: "Rooms",
                principalColumn: "RoomID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
