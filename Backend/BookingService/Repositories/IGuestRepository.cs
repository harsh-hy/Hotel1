using BookingService.Models;

namespace BookingService.Repositories;

public interface IGuestRepository
{
    Task<List<Guest>> GetAllAsync();
    Task<Guest?> GetByIdAsync(int id);
    Task<bool> HasReservationsAsync(int guestId);
    Task AddAsync(Guest guest);
    Task SaveChangesAsync();
}
