using BookingService.DTOs;

namespace BookingService.Services;

public interface IDashboardService
{
    Task<ServiceResult<DashboardResponseDto>> GetDashboardAsync(string role);
}
