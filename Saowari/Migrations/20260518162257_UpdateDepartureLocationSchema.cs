using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartureLocationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "DepartureLocation");

            migrationBuilder.AddColumn<int>(
                name: "LocationID",
                table: "DepartureLocation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Time",
                table: "DepartureLocation",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_DepartureLocation_LocationID",
                table: "DepartureLocation",
                column: "LocationID");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartureLocation_Location_LocationID",
                table: "DepartureLocation",
                column: "LocationID",
                principalTable: "Location",
                principalColumn: "LocationID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartureLocation_Location_LocationID",
                table: "DepartureLocation");

            migrationBuilder.DropIndex(
                name: "IX_DepartureLocation_LocationID",
                table: "DepartureLocation");

            migrationBuilder.DropColumn(
                name: "LocationID",
                table: "DepartureLocation");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "DepartureLocation");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DepartureLocation",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
