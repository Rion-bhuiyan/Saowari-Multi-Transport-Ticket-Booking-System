using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saowari.Migrations
{
    /// <inheritdoc />
    public partial class ShiftDepartureLocationToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the old FK constraint from Route
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DepartureLocation_Route_RouteID')
                    ALTER TABLE [DepartureLocation] DROP CONSTRAINT [FK_DepartureLocation_Route_RouteID];
            ");

            // 2. Drop old index on RouteID
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DepartureLocation_RouteID' AND object_id = OBJECT_ID('DepartureLocation'))
                    DROP INDEX [IX_DepartureLocation_RouteID] ON [DepartureLocation];
            ");

            // 3. Rename column RouteID -> ScheduleID
            migrationBuilder.RenameColumn(
                name: "RouteID",
                table: "DepartureLocation",
                newName: "ScheduleID");

            // 4. Add new FK to Schedule
            migrationBuilder.AddForeignKey(
                name: "FK_DepartureLocation_Schedule_ScheduleID",
                table: "DepartureLocation",
                column: "ScheduleID",
                principalTable: "Schedule",
                principalColumn: "ScheduleID",
                onDelete: ReferentialAction.Cascade);

            // 5. Recreate index on new ScheduleID column
            migrationBuilder.CreateIndex(
                name: "IX_DepartureLocation_ScheduleID",
                table: "DepartureLocation",
                column: "ScheduleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new FK if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DepartureLocation_Schedule_ScheduleID')
                    ALTER TABLE [DepartureLocation] DROP CONSTRAINT [FK_DepartureLocation_Schedule_ScheduleID];
            ");

            // Drop new index if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DepartureLocation_ScheduleID' AND object_id = OBJECT_ID('DepartureLocation'))
                    DROP INDEX [IX_DepartureLocation_ScheduleID] ON [DepartureLocation];
            ");

            // Rename column back (only if ScheduleID exists)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ScheduleID' AND object_id = OBJECT_ID('DepartureLocation'))
                BEGIN
                    EXEC sp_rename 'DepartureLocation.ScheduleID', 'RouteID', 'COLUMN';
                END
            ");

            // Restore old FK to Route
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DepartureLocation_Route_RouteID')
                    ALTER TABLE [DepartureLocation] ADD CONSTRAINT [FK_DepartureLocation_Route_RouteID]
                    FOREIGN KEY ([RouteID]) REFERENCES [Route] ([RouteID]) ON DELETE CASCADE;
            ");

            // Restore old index
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DepartureLocation_RouteID' AND object_id = OBJECT_ID('DepartureLocation'))
                    CREATE INDEX [IX_DepartureLocation_RouteID] ON [DepartureLocation] ([RouteID]);
            ");
        }
    }
}
