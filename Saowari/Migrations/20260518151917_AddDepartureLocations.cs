using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartureLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard: only create the table if it does not already exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DepartureLocation')
                BEGIN
                    CREATE TABLE [DepartureLocation] (
                        [DepartureLocationID] int NOT NULL IDENTITY,
                        [RouteID] int NOT NULL,
                        [Name] nvarchar(200) NOT NULL,
                        CONSTRAINT [PK_DepartureLocation] PRIMARY KEY ([DepartureLocationID]),
                        CONSTRAINT [FK_DepartureLocation_Route_RouteID]
                            FOREIGN KEY ([RouteID]) REFERENCES [Route] ([RouteID]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_DepartureLocation_RouteID]
                        ON [DepartureLocation] ([RouteID]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartureLocation");
        }
    }
}
