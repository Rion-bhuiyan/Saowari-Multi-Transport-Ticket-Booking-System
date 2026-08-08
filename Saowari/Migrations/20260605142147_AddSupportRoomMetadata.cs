using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportRoomMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrowserInfo",
                table: "SupportRoom",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Geolocation",
                table: "SupportRoom",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "SupportRoom",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IspName",
                table: "SupportRoom",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrowserInfo",
                table: "SupportRoom");

            migrationBuilder.DropColumn(
                name: "Geolocation",
                table: "SupportRoom");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "SupportRoom");

            migrationBuilder.DropColumn(
                name: "IspName",
                table: "SupportRoom");
        }
    }
}
