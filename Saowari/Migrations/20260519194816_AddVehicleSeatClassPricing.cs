using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleSeatClassPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeatPricing_SeatClasses_SeatClassId",
                table: "SeatPricing");

            migrationBuilder.Sql("DELETE FROM [SeatPricing]");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "SeatPricing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ScheduleSeatClassPricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    SeatClassId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSeatClassPricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleSeatClassPricing_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleSeatClassPricing_SeatClasses_SeatClassId",
                        column: x => x.SeatClassId,
                        principalTable: "SeatClasses",
                        principalColumn: "SeatClassId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatPricing_VehicleId_SeatClassId",
                table: "SeatPricing",
                columns: new[] { "VehicleId", "SeatClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSeatClassPricing_ScheduleId_SeatClassId",
                table: "ScheduleSeatClassPricing",
                columns: new[] { "ScheduleId", "SeatClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSeatClassPricing_SeatClassId",
                table: "ScheduleSeatClassPricing",
                column: "SeatClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_SeatPricing_SeatClasses_SeatClassId",
                table: "SeatPricing",
                column: "SeatClassId",
                principalTable: "SeatClasses",
                principalColumn: "SeatClassId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SeatPricing_Vehicle_VehicleId",
                table: "SeatPricing",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "VehicleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeatPricing_SeatClasses_SeatClassId",
                table: "SeatPricing");

            migrationBuilder.DropForeignKey(
                name: "FK_SeatPricing_Vehicle_VehicleId",
                table: "SeatPricing");

            migrationBuilder.DropTable(
                name: "ScheduleSeatClassPricing");

            migrationBuilder.DropIndex(
                name: "IX_SeatPricing_VehicleId_SeatClassId",
                table: "SeatPricing");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "SeatPricing");

            migrationBuilder.AddForeignKey(
                name: "FK_SeatPricing_SeatClasses_SeatClassId",
                table: "SeatPricing",
                column: "SeatClassId",
                principalTable: "SeatClasses",
                principalColumn: "SeatClassId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
