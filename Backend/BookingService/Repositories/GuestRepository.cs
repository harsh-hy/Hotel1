using BookingService.Data;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories;

public class GuestRepository(BookingDbContext db) : IGuestRepository
{
    public Task<List<Guest>> GetAllAsync() =>
        db.Guests.AsNoTracking().OrderBy(g => g.FullName).ToListAsync();

    public Task<Guest?> GetByIdAsync(int id) =>
        db.Guests.FindAsync(id).AsTask();

    public Task<bool> HasReservationsAsync(int guestId) =>
        db.Reservations.AnyAsync(r => r.GuestId == guestId && r.ReservationStatus != "Cancelled");

    public async Task AddAsync(Guest guest) => await db.Guests.AddAsync(guest);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
