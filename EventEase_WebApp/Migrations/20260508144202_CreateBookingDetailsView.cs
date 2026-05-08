using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase_WebApp.Migrations
{
    /// <inheritdoc />
    public partial class CreateBookingDetailsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[vwBookingDetails]', N'V') IS NOT NULL
                    DROP VIEW [dbo].[vwBookingDetails];
                """);
        }
    }
}
