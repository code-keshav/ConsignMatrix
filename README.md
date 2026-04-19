# ConsignMatrix

A consignment tracking system built for managing end-to-end shipment operations — from booking and pickup through branch processing, transit, and last-mile delivery.

## About

ConsignMatrix handles the full lifecycle of a consignment in a hub-and-spoke logistics network. Shipments are booked at a branch, picked up from the sender, processed through inward/sort/bag/dispatch stages, moved via trips between branches, and finally delivered to the receiver. Every status change is logged, creating a complete audit trail per consignment.

The system supports multiple branches, each with their own staff, vehicles, and service areas (mapped via pin codes). Role-based access control ensures users only see and do what they're supposed to.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10.0, ASP.NET Core MVC |
| ORM | Entity Framework Core 10.0 |
| Database | PostgreSQL |
| Frontend | AdminLTE 4, Bootstrap 5, jQuery |
| Reactive UI | Vue.js 3 (used on the Pickup dashboard) |
| Auth | Cookie authentication + custom ACL permission system |
| Icons | FontAwesome 6 |
| Alerts | SweetAlert2 |

## Features

**Authentication & Authorization**
- Cookie-based session authentication
- Custom ACL permission system loaded from a JSON config
- Four user levels: SuperAdmin, Admin, BranchAdmin, User
- Role-permission assignment UI with grouped checkboxes

**User & Branch Management**
- User CRUD with Excel import and password management
- Branch setup with pin code mapping for serviceability checks
- User branch transfer workflow (request → approve/reject)

**Employee & Fleet**
- Employee records with optional user account linking
- Driver management with license tracking
- Vehicle registration with capacity, type, and special capability flags
- Vehicle-driver assignment with date ranges

**Customer Management**
- Customer profiles (individual and business) with multiple addresses
- Address management with GPS coordinates

**Consignment Booking**
- Auto-generated tracking numbers
- Multi-package support with per-package dimensions and weight
- Volumetric weight calculation
- Service type and payment mode selection
- Destination serviceability check via pin code

**Pickup Operations**
- Schedule pickups with date and time slot
- Driver + vehicle assignment (single and bulk)
- Start, complete, or fail a pickup task
- Auto-reschedule on failure (up to 3 attempts)
- Pickup dashboard with filters and summary cards (Vue.js)

**Branch Operations (Sender Side)**
- Single-page workflow with tabs: Inward → Sort → Bag → Dispatch
- Bulk actions with running weight totals
- Search by tracking number, filter by destination branch
- Summary cards per queue

**Branch Operations (Receiver Side)**
- Incoming trip visibility with direction-based filtering
- Scan-to-verify: mark each consignment as Received, Damaged, or Missing
- Trip completion blocked until every manifest item is processed
- Ready-for-Delivery queue for consignments at destination

**Trip Management**
- Trip creation for LineHaul, Transfer, and Delivery types
- Driver/vehicle validation against branch and availability
- Start trip → consignments move to InTransit
- Delivery trips: start → OutForDelivery, mark delivered or failed
- Failed deliveries return to the ready queue for the next run

**Delivery Execution**
- Per-consignment delivery marking with receiver name and remarks
- Failure reasons: CustomerNotAvailable, AddressNotFound, CustomerRefused, etc.
- Complete delivery trip only when all items are processed

**Dashboard**
- Summary cards showing counts per status stage
- Quick navigation to operational views

## Project Structure

The solution has three projects:

```
ConsignMatrix/
├── Base/                   # Core domain layer
│   ├── Entities/           # All entity classes (User, Branch, Consignment, Trip, etc.)
│   ├── Enum/               # Enums grouped by module
│   ├── Repo/               # Generic repository + specific repos
│   ├── Services/           # Business logic (one service per module)
│   ├── Providers/          # CurrentUserProvider, ContentPathProvider
│   └── Configuration/      # DI registration
│
├── Acl/                    # Permission system
│   ├── Entities/           # RolePermission
│   ├── Helper/             # PermissionProvider, PermissionChecker, PermissionHandler
│   └── Configuration/      # DI registration
│
└── Web/                    # ASP.NET Core MVC app
    ├── Areas/
    │   ├── Admin/          # User, Branch, Employee, Vehicle, Organization controllers
    │   ├── Acl/            # Role, RolePermission, UserRole controllers
    │   └── Consignment/    # Customer, Consignment, Pickup, BranchOps, Trip controllers
    ├── Views/              # Razor views + shared layouts
    ├── wwwroot/            # Static assets (AdminLTE, CSS, JS)
    ├── Acl.json            # Permission definitions
    ├── AppDbContext.cs      # EF Core context with auto-auditing
    ├── Migrations/          # EF Core migrations
```

**Architecture patterns used:**
- Generic Repository + Unit of Work
- Service layer for business logic
- Soft delete via `ISoftDelete` interface (marks records instead of deleting)
- Base entity with automatic audit fields (timestamp, user, branch)
- Snake_case database naming convention

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (tested with v15+)

No separate Node.js setup needed — all frontend assets are included in `wwwroot/`.

## Getting Started

1. **Clone the repository**

   ```bash
   git clone <repo-url>
   cd ConsignMatrix
   ```

2. **Set up the database**

   Create a PostgreSQL database (e.g. `consign_matrix`) and a user with access to it.

3. **Create a development config**

   Copy or create `Web/appsettings.development.json`:

   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost; Port=5432; Username=postgres; Password=yourpassword; Database=consign_matrix;"
     },
     "ContentDir": "C:\\path\\to\\content\\folder"
   }
   ```

   `ContentDir` is where uploaded files (like org logos) get stored. Point it to any folder on your machine.

4. **Run the app**

   ```bash
   cd Web
   dotnet restore
   dotnet run
   ```

   The app runs at **http://localhost:5180** by default.

   Database migrations are applied automatically on startup — no need to run `dotnet ef database update` manually.

5. **First login**

   On a fresh database, you'll need to seed a SuperAdmin user. Use `/admin/user/registerinitial` or create one directly in the database.

## Database

PostgreSQL with three schemas:

| Schema | Purpose |
|--------|---------|
| `base` | Users, branches, employees, vehicles, organization |
| `acl` | Roles, role permissions |
| `consignment` | Customers, consignments, packages, pickup tasks, trips |

All table and column names use **snake_case** (handled automatically by `EFCore.NamingConventions`).

Entities that implement `ISoftDelete` are never physically deleted — they're flagged with `rec_status = 'D'` and filtered out by a global query filter.

## Permissions

Permissions are defined in `Web/Acl.json`. Each permission maps to a specific URL + HTTP method and belongs to a group. At startup, the app reads this file and creates authorization policies dynamically.

To add a new permission:
1. Add an entry in `Acl.json`
2. Assign the permission to roles through the UI
3. Restart the app (permissions are loaded once at startup)

## Consignment Status Flow

```
Booked
  → PickupScheduled → PickedUp (or PickupAttempted on failure)
    → ReceivedAtOrigin
      → Sorted → Bagged → Dispatched
        → InTransit
          → ReceivedAtDestination (or Damaged / Lost)
            → OutForDelivery
              → Delivered (or DeliveryAttempted on failure)
```

Each transition creates an immutable `ConsignmentStatusLog` entry. The current status is always derived from the latest log row.

## Development Notes

- Migrations auto-run on startup — be careful with production deployments
- Lazy loading is enabled (EF Core Proxies) — watch for N+1 queries
- Audit fields (`rec_date`, `rec_by_id`, `rec_branch_id`) are set automatically in `SaveChangesAsync`
- The permission system reads `Acl.json` once at startup — changes need a restart
