using BookingService.DTOs;

namespace BookingService.Services;

public interface IBillService
{
    Task<ServiceResult<BillResponseDto>> GetBillAsync(int reservationId, string? jwtToken);
}
