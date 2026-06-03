using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleWorkflowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleApplication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DepartureDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleApplication_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleApplication_Route_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Route",
                        principalColumn: "RouteID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleApplication_Schedule_CreatedScheduleId",
                        column: x => x.CreatedScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleApplication_User_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleApplication_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleChatRemovedUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RemovedByUserId = table.Column<int>(type: "int", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleChatRemovedUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleChatRemovedUser_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleChatRemovedUser_User_RemovedByUserId",
                        column: x => x.RemovedByUserId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleChatRemovedUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleExchangeRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    RequesterScheduleId = table.Column<int>(type: "int", nullable: false),
                    TargetUserId = table.Column<int>(type: "int", nullable: false),
                    TargetScheduleId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ManagerRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeerRespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerRespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleExchangeRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleExchangeRequest_Schedule_RequesterScheduleId",
                        column: x => x.RequesterScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleExchangeRequest_Schedule_TargetScheduleId",
                        column: x => x.TargetScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleExchangeRequest_User_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleExchangeRequest_User_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleApplication_CompanyId",
                table: "ScheduleApplication",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleApplication_CreatedScheduleId",
                table: "ScheduleApplication",
                column: "CreatedScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleApplication_RequesterId",
                table: "ScheduleApplication",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleApplication_RouteId",
                table: "ScheduleApplication",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleApplication_VehicleId",
                table: "ScheduleApplication",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChatRemovedUser_RemovedByUserId",
                table: "ScheduleChatRemovedUser",
                column: "RemovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChatRemovedUser_ScheduleId_UserId",
                table: "ScheduleChatRemovedUser",
                columns: new[] { "ScheduleId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleChatRemovedUser_UserId",
                table: "ScheduleChatRemovedUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExchangeRequest_RequesterId",
                table: "ScheduleExchangeRequest",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExchangeRequest_RequesterScheduleId",
                table: "ScheduleExchangeRequest",
                column: "RequesterScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExchangeRequest_TargetScheduleId",
                table: "ScheduleExchangeRequest",
                column: "TargetScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExchangeRequest_TargetUserId",
                table: "ScheduleExchangeRequest",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleApplication");

            migrationBuilder.DropTable(
                name: "ScheduleChatRemovedUser");

            migrationBuilder.DropTable(
                name: "ScheduleExchangeRequest");
        }
    }
}
