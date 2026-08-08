using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddTrafficAnalyticsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "UserLoginHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "UserLoginHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "UserLoginHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "UserLoginHistory",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IspName",
                table: "UserLoginHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referrer",
                table: "UserLoginHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrafficChannel",
                table: "UserLoginHistory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "City",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "IspName",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "Referrer",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "TrafficChannel",
                table: "UserLoginHistory");
        }
    }
}
