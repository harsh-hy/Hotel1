using BookingService.Models;

namespace BookingService.Repositories;

public interface IReservationRepository
{
    Task<List<Reservation>> GetAllWithGuestAsync();
    Task<Reservation?> GetByIdWithGuestAndPaymentsAsync(int id);
    Task<List<Reservation>> GetTodayReservationsAsync(DateOnly today);
    Task<bool> HasOverlapAsync(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeReservationId = null);
    Task<bool> GuestExistsAsync(int guestId);
    Task AddAsync(Reservation reservation);
    Task SaveChangesAsync();
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginSerializableTransactionAsync();
}
