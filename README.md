# Petrol Station Management System (PSMS)

C# WinForms desktop application (.NET, strict MVC) with a PostgreSQL database. University lab project (course: .NET).

## Prerequisites
- .NET SDK matching the project target (`net10.0-windows`, see `PetrolStationMS.csproj`) and Windows
- PostgreSQL 14+ running on `localhost:5432` with `psql` on the PATH
- Visual Studio 2022+ (optional) or the `dotnet` CLI

## Setup
1. Create the database (once):
   ```
   psql -U postgres -h localhost -c "CREATE DATABASE psms;"
   ```
2. Apply the schema and seed data (safe to re-run):
   ```
   psql -U postgres -h localhost -d psms -f Data/schema.sql
   ```
3. Create your configuration file and set the real password:
   ```
   copy appsettings.example.json appsettings.json
   ```
   Edit `appsettings.json` -> `ConnectionStrings:Default`. This file is git-ignored; never commit it.
4. Run:
   ```
   dotnet run
   ```
   (or open `PetrolStationMS.slnx` in Visual Studio and press F5).

## Default logins
| Username  | Password       | Role      | Can use |
|-----------|----------------|-----------|---------|
| admin     | Admin@123      | Admin     | everything, incl. user administration |
| manager   | Manager@123    | Manager   | everything except user administration |
| cashier   | Cashier@123    | Cashier   | payments, customers, expenses, reconciliation, sales view, dashboard |
| attendant | Attendant@123  | Attendant | shifts / pump readings only |

Change these passwords (Master Data > Users > Reset Password) before any real use.

## Project layout (MVC)
- `Models/` plain classes (User, Tank, Sale, Payment, ...)
- `Views/` WinForms forms only: display, collect input, call controllers, show messages (no SQL, no business rules)
- `Controllers/` validation + business rules, transactions, friendly error results, audit calls
- `Data/` `Db.cs` (connection + transaction helper), one Dapper repository per entity, `schema.sql`
- `Session.cs` logged-in user, role and the role-permission map
- `Program.cs` loads config, shows login, then the main window

## Typical workflow to try
1. Log in as `manager`: set prices (Master Data > Fuel Prices), record a delivery (Purchasing > Deliveries).
2. Log in as `attendant`: Operations > Shifts: open a shift with opening readings, later close it with closing readings (creates sales, lowers tank stock).
3. Log in as `cashier`: Finance > Payments (cash / mobile money / card / credit), Receive Credit Payment, Shift Reconciliation, Expenses.
4. Log in as `manager`: Reports, Audit Log, Void Transactions.

## Notes
- Completed transactions are never deleted; sales, deliveries and payments are voided (reason required, Manager/Admin only) and the stock/balance effects are reversed in one transaction.
- Void order: payments linked to a sale must be voided before the sale; a credit payment can only be voided while the customer balance still covers it (void later credit receipts first).
- See `docs/REQUIREMENTS_COVERAGE.md` for which SRS requirements are covered.
