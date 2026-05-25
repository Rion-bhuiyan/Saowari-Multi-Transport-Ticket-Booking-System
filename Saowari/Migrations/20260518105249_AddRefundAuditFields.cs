using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Refund",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Refund",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Refund_UpdatedByUserId",
                table: "Refund",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Refund_User_UpdatedByUserId",
                table: "Refund",
                column: "UpdatedByUserId",
                principalTable: "User",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Refund_User_UpdatedByUserId",
                table: "Refund");

            migrationBuilder.DropIndex(
                name: "IX_Refund_UpdatedByUserId",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Refund");
        }
    }
}
