using BookingService.Models;

namespace BookingService.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<List<Payment>> GetByReservationIdAsync(int reservationId);
    Task<bool> ReservationExistsAsync(int reservationId);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}
