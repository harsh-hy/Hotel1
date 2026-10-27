# Online Hotel Management System — Backend

Backend scaffold aligned to the supplied OHMS source. ASP.NET Core Web APIs, EF Core SQL Server, JWT/RBAC, Ocelot, Serilog, Swagger and NUnit/Moq.

## Prerequisites
- .NET 8 SDK
- SQL Server / LocalDB
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Services
- `Backend/IdentityService` → IdentityDB
- `Backend/HotelService` → HotelDB
- `Backend/BookingService` → BookingDB
- `Backend/ApiGateway` → Ocelot routes
- `Tests/*` → NUnit + Moq test projects

## Configure and run
1. Set each service's connection string and JWT settings in `appsettings.Development.json` or environment variables. Never commit production secrets.
2. From each service directory run `dotnet restore`, then `dotnet ef migrations add InitialCreate`, `dotnet ef database update`, and `dotnet run`.
3. Run gateway after all three services. Gateway routes use ports 5101/5102/5103; update if your launch settings differ.
4. Create the first Owner through the documented secure seed/setup procedure before normal use. Do not enable public registration.

## Important
This is a generated starter backend, not a claim of a production-verified system. This environment has no .NET SDK, so compilation and tests could not be executed here. Review TODOs and complete hardening/concurrency tests before submission. The implementation intentionally excludes message brokers, real payment processing, customer login, a Billing service, and a Bills table. Billing belongs in BookingService.
