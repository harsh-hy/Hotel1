using BookingService.DTOs;

namespace BookingService.Services;

public interface IPaymentService
{
    Task<ServiceResult<PaymentResponseDto>> RecordPaymentAsync(MockPaymentRequestDto dto);
    Task<ServiceResult<PaymentResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<PaymentResponseDto>>> GetByReservationIdAsync(int reservationId);
}
