using BookingService.Data;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingService.Repositories;

public class ReservationRepository(BookingDbContext db) : IReservationRepository
{
    public Task<List<Reservation>> GetAllWithGuestAsync() =>
        db.Reservations.AsNoTracking()
            .Include(r => r.Guest)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<Reservation?> GetByIdWithGuestAndPaymentsAsync(int id) =>
        db.Reservations
            .Include(r => r.Guest)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

    public Task<List<Reservation>> GetTodayReservationsAsync(DateOnly today) =>
        db.Reservations.AsNoTracking()
            .Include(r => r.Guest)
            .Where(r => r.CheckInDate == today || r.CheckOutDate == today)
            .OrderBy(r => r.CheckInDate)
            .ToListAsync();

    public Task<bool> HasOverlapAsync(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeReservationId = null) =>
        db.Reservations.AnyAsync(r =>
            r.RoomId == roomId &&
            r.ReservationStatus != "Cancelled" &&
            r.CheckInDate < checkOut &&
            r.CheckOutDate > checkIn &&
            (excludeReservationId == null || r.ReservationId != excludeReservationId));

    public Task<bool> GuestExistsAsync(int guestId) =>
        db.Guests.AnyAsync(g => g.GuestId == guestId);

    public async Task AddAsync(Reservation reservation) =>
        await db.Reservations.AddAsync(reservation);

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    public Task<IDbContextTransaction> BeginSerializableTransactionAsync() =>
        db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
}
