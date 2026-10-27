using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;

namespace BookingService.Services;

public class PaymentService(IPaymentRepository paymentRepo, IReservationRepository reservationRepo) : IPaymentService
{
    private static readonly string[] ValidMethods = ["Cash", "Card", "UPI"];

    public async Task<ServiceResult<PaymentResponseDto>> RecordPaymentAsync(MockPaymentRequestDto dto)
    {
        if (dto.Amount <= 0)
            return ServiceResult<PaymentResponseDto>.Fail("Payment amount must be positive.", 400);
        if (!ValidMethods.Contains(dto.PaymentMethod))
            return ServiceResult<PaymentResponseDto>.Fail("Payment method must be Cash, Card, or UPI.", 400);

        var reservation = await reservationRepo.GetByIdWithGuestAndPaymentsAsync(dto.ReservationId);
        if (reservation is null)
            return ServiceResult<PaymentResponseDto>.Fail("Reservation not found.", 404);
        if (reservation.ReservationStatus == "Cancelled")
            return ServiceResult<PaymentResponseDto>.Fail("Cannot record payment for a cancelled reservation.", 409);

        var paidSoFar = reservation.Payments.Where(p => p.PaymentStatus == "Paid").Sum(p => p.Amount);
        var outstanding = reservation.TotalAmount - paidSoFar;
        if (dto.Amount > outstanding)
            return ServiceResult<PaymentResponseDto>.Fail(
                $"Payment of {dto.Amount} exceeds outstanding balance of {outstanding}.", 409);

        var payment = new Payment
        {
            ReservationId = reservation.ReservationId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = "Paid",
            TransactionReference = "OHMS-" + Guid.NewGuid().ToString("N").ToUpperInvariant(),
            PaymentDate = DateTime.UtcNow
        };
        await paymentRepo.AddAsync(payment);
        await paymentRepo.SaveChangesAsync();
        return ServiceResult<PaymentResponseDto>.Ok(Map(payment));
    }

    public async Task<ServiceResult<PaymentResponseDto>> GetByIdAsync(int id)
    {
        var p = await paymentRepo.GetByIdAsync(id);
        return p is null
            ? ServiceResult<PaymentResponseDto>.Fail("Payment not found.", 404)
            : ServiceResult<PaymentResponseDto>.Ok(Map(p));
    }

    public async Task<ServiceResult<List<PaymentResponseDto>>> GetByReservationIdAsync(int reservationId)
    {
        if (!await paymentRepo.ReservationExistsAsync(reservationId))
            return ServiceResult<List<PaymentResponseDto>>.Fail("Reservation not found.", 404);

        var payments = await paymentRepo.GetByReservationIdAsync(reservationId);
        return ServiceResult<List<PaymentResponseDto>>.Ok(payments.Select(Map).ToList());
    }

    private static PaymentResponseDto Map(Payment p) => new()
    {
        PaymentId = p.PaymentId,
        ReservationId = p.ReservationId,
        Amount = p.Amount,
        PaymentMethod = p.PaymentMethod,
        PaymentStatus = p.PaymentStatus,
        TransactionReference = p.TransactionReference,
        PaymentDate = p.PaymentDate
    };
}
