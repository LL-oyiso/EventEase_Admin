using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventEase_WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeAndVenueAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venues",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTypeName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "EventTypeName" },
                values: new object[,]
                {
                    { 1, "Conference" },
                    { 2, "Wedding" },
                    { 3, "Concert" },
                    { 4, "Festival" },
                    { 5, "Corporate" },
                    { 6, "Private" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_EventTypeName",
                table: "EventTypes",
                column: "EventTypeName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[vwBookingDetails]', N'V') IS NOT NULL
                    DROP VIEW [dbo].[vwBookingDetails];
                """);

            migrationBuilder.Sql(
                """
                CREATE VIEW [dbo].[vwBookingDetails]
                AS
                SELECT
                    b.BookingId,
                    b.BookingDate,
                    b.EventId,
                    e.EventName,
                    e.EventDate,
                    e.EventTypeId,
                    COALESCE(et.EventTypeName, 'Unclassified') AS EventTypeName,
                    b.VenueId,
                    v.VenueName,
                    v.Location AS VenueLocation,
                    v.IsAvailable AS VenueIsAvailable
                FROM dbo.Bookings AS b
                INNER JOIN dbo.Events AS e ON e.EventId = b.EventId
                INNER JOIN dbo.Venues AS v ON v.VenueId = b.VenueId
                LEFT JOIN dbo.EventTypes AS et ON et.EventTypeId = e.EventTypeId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[vwBookingDetails]', N'V') IS NOT NULL
                    DROP VIEW [dbo].[vwBookingDetails];
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventTypeId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Events");

            migrationBuilder.Sql(
                """
                CREATE VIEW [dbo].[vwBookingDetails]
                AS
                SELECT
                    b.BookingId,
                    b.BookingDate,
                    b.EventId,
                    e.EventName,
                    e.EventDate,
                    b.VenueId,
                    v.VenueName,
                    v.Location AS VenueLocation
                FROM dbo.Bookings AS b
                INNER JOIN dbo.Events AS e ON e.EventId = b.EventId
                INNER JOIN dbo.Venues AS v ON v.VenueId = b.VenueId;
                """);
        }
    }
}
