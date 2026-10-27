using BookingService.DTOs;

namespace BookingService.Services;

public interface IReservationService
{
    Task<ServiceResult<List<ReservationResponseDto>>> GetAllAsync();
    Task<ServiceResult<ReservationResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<ReservationResponseDto>>> GetTodayAsync();
    Task<ServiceResult<ReservationResponseDto>> CreateAsync(CreateReservationRequestDto dto, string? jwtToken);
    Task<ServiceResult<ReservationResponseDto>> UpdateAsync(int id, UpdateReservationRequestDto dto, string? jwtToken);
    Task<ServiceResult<bool>> CancelAsync(int id);
    Task<ServiceResult<bool>> CheckInAsync(int id);
    Task<ServiceResult<bool>> CheckOutAsync(int id);
}
