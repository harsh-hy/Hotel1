using BookingService.Data;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories;

public class PaymentRepository(BookingDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(int id) =>
        db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentId == id);

    public Task<List<Payment>> GetByReservationIdAsync(int reservationId) =>
        db.Payments.AsNoTracking()
            .Where(p => p.ReservationId == reservationId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();

    public Task<bool> ReservationExistsAsync(int reservationId) =>
        db.Reservations.AnyAsync(r => r.ReservationId == reservationId);

    public async Task AddAsync(Payment payment) => await db.Payments.AddAsync(payment);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
