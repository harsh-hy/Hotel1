# Implementation Status

## ✅ Complete

All 37 public API endpoints are implemented across the three microservices and routed through the Ocelot gateway.

### Identity Service (port 5101)
- POST /api/identity/login — JWT login, inactive-user check
- GET  /api/identity/me — authenticated profile
- POST /api/identity/change-password — current-password verification
- GET  /api/identity/users — Owner only
- GET  /api/identity/users/{id} — Owner only
- POST /api/identity/users — Owner; creates Manager/Receptionist only
- PUT  /api/identity/users/{id} — Owner; all roles including Owner
- Owner account seeded on first startup via appsettings.json `Owner:*` keys
- EF Core migrations run on startup

### Hotel Service (port 5102)
- GET/POST/PUT/DELETE /api/hotel/rooms — full CRUD
  - Safe deletion: calls BookingService internal endpoint to check active reservations; hard-deletes only if none; soft-deactivates if BookingService is unavailable
- PUT /api/hotel/rooms/{id}/price — dedicated price update
- GET/POST/PUT/DELETE /api/hotel/room-types — full CRUD; delete blocked if rooms assigned
- GET /api/hotel/rooms/available — calls BookingService for occupied room IDs, filters by status/capacity
- GET /api/hotel/rooms/{id}/availability — single-room availability for a date range
- Named HttpClient "BookingService" registered; base URL from `Services:BookingService` in appsettings

### Booking Service (port 5103)
- GET/POST/PUT /api/booking/guests — full CRUD
- GET/POST /api/booking/reservations — list and create
  - Create: validates via HotelService (room status + capacity), uses serializable transaction for conflict check, snapshots price
- PUT /api/booking/reservations/{id} — update dates/guests (Booked only), concurrency-safe
- GET /api/booking/reservations/today — reservations checking in or out today
- POST /api/booking/reservations/{id}/cancel — Booked → Cancelled
- POST /api/booking/reservations/{id}/check-in — Booked → CheckedIn
- POST /api/booking/reservations/{id}/check-out — CheckedIn → CheckedOut
- POST /api/booking/payments/mock — mock payment; prevents overpayment and cancelled-reservation payment
- GET  /api/booking/payments/{id}
- GET  /api/booking/reservations/{id}/payments
- GET  /api/booking/reservations/{id}/bill — bill with guest/room details, nights, snapshot price, paid/outstanding
- GET  /api/booking/dashboard — role-appropriate (Receptionist: ops only; Owner/Manager: + revenue)
- GET  /api/booking/reports/revenue — ?from=&to=
- GET  /api/booking/reports/reservations — ?from=&to=&status=
- Internal (not exposed via gateway): /api/booking/internal/occupied-rooms, /api/booking/internal/rooms/{id}/has-active-reservations

### Api Gateway (port 5000)
- Routes all /api/identity/*, /api/hotel/*, /api/booking/* to respective services
- /api/booking/internal/* is blocked at gateway level (AuthenticationProviderKey)
- Serilog console logging

### Cross-cutting
- Serilog structured logging on all services and gateway; config via appsettings.json Serilog section
- JWT bearer auth with same key/issuer/audience across all services
- Global exception handler (maps /error) on all services
- AddProblemDetails on all services
- EF Core migrations run automatically on startup

## Tests (NUnit + Moq) — 52 tests, all passing

- IdentityService.Tests: AuthControllerTests (4), UsersControllerTests (6) + SmokeTests (1)
- HotelService.Tests: RoomTypesControllerTests (5), RoomsControllerTests (6) + SmokeTests (1)
- BookingService.Tests: GuestsControllerTests (6), PaymentsControllerTests (8), ReservationsControllerTests (14) + SmokeTests (1)

## Remaining for Production Readiness (out of academic scope)
- Replace dev JWT key with a real secret via environment variable or secrets manager
- Add EF Core migration files (run `dotnet ef migrations add Initial` per service)
- Configure real SQL Server connection strings for non-localdb environments
- Add integration/end-to-end tests
- Add Swagger JWT bearer configuration for authenticated Swagger UI
- Add correlation ID middleware (X-Correlation-ID header propagation)
