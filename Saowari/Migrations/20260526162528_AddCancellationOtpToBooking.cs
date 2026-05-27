using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationOtpToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationOtp",
                table: "Booking",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationOtpExpiry",
                table: "Booking",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationOtp",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "CancellationOtpExpiry",
                table: "Booking");
        }
    }
}
