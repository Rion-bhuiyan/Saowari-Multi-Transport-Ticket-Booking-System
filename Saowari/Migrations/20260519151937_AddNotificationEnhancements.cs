using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_User_UserId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "ColorClass",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);



            migrationBuilder.CreateTable(
                name: "AdminNotificationPreference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminUserId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotificationPreference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminNotificationPreference_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotificationPreference_User_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotificationPreference_AdminUserId_CompanyId",
                table: "AdminNotificationPreference",
                columns: new[] { "AdminUserId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotificationPreference_CompanyId",
                table: "AdminNotificationPreference",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Company_CompanyId",
                table: "Notifications",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_User_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Company_CompanyId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_User_UserId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "AdminNotificationPreference");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Notifications");



            migrationBuilder.AlterColumn<string>(
                name: "ColorClass",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_User_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserID");
        }
    }
}
