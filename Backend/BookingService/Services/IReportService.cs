using BookingService.DTOs;

namespace BookingService.Services;

public interface IReportService
{
    Task<ServiceResult<RevenueReportResponseDto>> GetRevenueReportAsync(DateOnly? from, DateOnly? to);
    Task<ServiceResult<ReservationReportResponseDto>> GetReservationReportAsync(DateOnly? from, DateOnly? to, string? status);
}
