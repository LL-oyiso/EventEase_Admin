# EventEase Admin — Venue Booking System

A cloud-hosted venue and event booking administration platform built with ASP.NET Core MVC, Entity Framework Core, Azure SQL Database, and Azure Blob Storage.

**Live URL:** `https://eventease-admin-tosloy01-aagefsfvaxgng3g9.southafricanorth-01.azurewebsites.net`

---

## Features

- **Venue management** — Create, edit, view, and delete venues with availability tracking and Azure Blob image storage.
- **Event management** — Create, edit, view, and delete events with event-type classification, optional venue assignment, and image upload.
- **Booking management** — Capture, edit, view, and delete bookings with GUID-based references.
- **Business rule enforcement** — Double-booking prevention (same venue/date) and event–venue consistency validation.
- **Advanced booking filtering** — Filter bookings by event type, date range (from/to), venue availability, and free-text search.
- **SQL view reporting** — `vwBookingDetails` consolidates booking/event/venue data for efficient filter-based querying.
- **EventType lookup table** — Predefined categories (Conference, Wedding, Concert, Festival, Corporate, Private) seed from migration.
- **Dashboard** — Live data overview: total events, upcoming events, bookings, unavailable venues, 6-month booking trend, event type distribution, and top events.
- **Azure Blob Storage** — Images uploaded, replaced, and deleted with full lifecycle management.
- **Validation** — Server-side model validation, custom date-not-in-past attribute, and TempData notifications.
- **Modern admin UI** — Sidebar navigation, top header, responsive grid layout, and text-only alert styling.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Web framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core 9 |
| Database | Azure SQL Database |
| Media storage | Azure Blob Storage |
| Hosting | Azure App Service (Windows) |
| UI | Bootstrap 5 + Bootstrap Icons |
| Authentication | N/A (admin-only, no public auth) |

---

## Project Structure

```
EventEase_WebApp/
├── Controllers/
│   ├── HomeController.cs        # Dashboard data aggregation
│   ├── VenuesController.cs      # Venue CRUD + blob image management
│   ├── EventsController.cs      # Event CRUD + blob image management
│   └── BookingsController.cs    # Booking CRUD + advanced filtering
├── Models/
│   ├── Venue.cs
│   ├── Event.cs
│   ├── Booking.cs
│   ├── EventType.cs
│   └── BookingDetail.cs         # Keyless entity mapped to vwBookingDetails
├── ViewModels/
│   └── DashboardViewModel.cs
├── Services/
│   ├── IBlobStorageService.cs
│   └── BlobStorageService.cs
├── Data/
│   └── EventEaseDbContext.cs
├── Migrations/
├── Views/
│   ├── Home/Index.cshtml        # Dashboard
│   ├── Venues/
│   ├── Events/
│   ├── Bookings/
│   └── Shared/_Layout.cshtml   # Sidebar + topbar shell
└── wwwroot/css/site.css
```

---

## Local Development Setup

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (Visual Studio) or SQL Server
- Azure Storage Account (for blob features) or use placeholder URL in appsettings

### Run locally

```bash
git clone <repo-url>
cd EventEase_WebApp/EventEase_WebApp
dotnet restore
dotnet ef database update
dotnet run
```

### User secrets (blob storage — local only)

```bash
dotnet user-secrets set "AzureBlob:ConnectionString" "<your-storage-connection-string>"
dotnet user-secrets set "AzureBlob:ContainerName" "eventease-images"
```

---

## Azure Deployment

### App Service configuration (required)

Set the following in Azure Portal > App Service > Configuration:

| Setting | Type | Purpose |
|---|---|---|
| `EventEaseDb` | Connection string (SQLAzure) | Azure SQL connection string |
| `AzureBlob__ConnectionString` | Application setting | Blob Storage connection string |
| `AzureBlob__ContainerName` | Application setting | Blob container name |

### Run production migrations

```powershell
$cs = '<your-azure-sql-connection-string>'
dotnet ef database update --connection "$cs"
```

### Verify production schema

```sql
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;
SELECT * FROM EventTypes;
SELECT VenueId, VenueName, IsAvailable FROM Venues;
SELECT TOP 10 BookingId, EventTypeName, VenueIsAvailable FROM vwBookingDetails;
```

---

## Key Database Objects

| Object | Type | Purpose |
|---|---|---|
| `Venues` | Table | Venue records with availability flag |
| `Events` | Table | Event records with EventTypeId FK |
| `Bookings` | Table | Booking records with unique venue/date constraint |
| `EventTypes` | Table | Predefined event classification lookup |
| `vwBookingDetails` | View | Consolidated join for booking display and filtering |
| `__EFMigrationsHistory` | Table | EF Core migration tracking |

---

## Security Notes

- All sensitive configuration is managed via Azure App Service settings — never committed to source control.
- `dotnet user-secrets` is used for local development secrets.
- SQL credentials should be rotated periodically and immediately after any unintended exposure.
- No authentication layer is implemented in this version (admin-only intended use).

---

## License

Academic project — CLDV7111, 2026.
