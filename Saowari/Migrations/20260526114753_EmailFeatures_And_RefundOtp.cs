using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class EmailFeatures_And_RefundOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // removed IsActive due to conflict

            migrationBuilder.AddColumn<string>(
                name: "EmailChangeOtpCode",
                table: "User",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailChangeOtpExpireTime",
                table: "User",
                type: "datetime2",
                nullable: true);

            // removed LoginOtpCode and LoginOtpExpireTime

            migrationBuilder.AddColumn<string>(
                name: "PendingNewEmail",
                table: "User",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundOtpCode",
                table: "Refund",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundOtpExpireTime",
                table: "Refund",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemSetting",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSetting", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSetting");

            // removed IsActive from Down method

            migrationBuilder.DropColumn(
                name: "EmailChangeOtpCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EmailChangeOtpExpireTime",
                table: "User");

            // removed LoginOtpCode and LoginOtpExpireTime from Down method

            migrationBuilder.DropColumn(
                name: "PendingNewEmail",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RefundOtpCode",
                table: "Refund");

            migrationBuilder.DropColumn(
                name: "RefundOtpExpireTime",
                table: "Refund");
        }
    }
}
