using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationOtpAndVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationOtpCode",
                table: "User",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationOtpExpireTime",
                table: "User",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RegistrationOtpCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RegistrationOtpExpireTime",
                table: "User");
        }
    }
}
