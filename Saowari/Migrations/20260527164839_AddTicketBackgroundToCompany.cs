using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketBackgroundToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TicketBackgroundOpacity",
                table: "Company",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TicketBackgroundUrl",
                table: "Company",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketBackgroundOpacity",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "TicketBackgroundUrl",
                table: "Company");
        }
    }
}
