using BookingService.DTOs;

namespace BookingService.Services;

public interface IGuestService
{
    Task<ServiceResult<List<GuestResponseDto>>> GetAllAsync();
    Task<ServiceResult<GuestResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<GuestResponseDto>> CreateAsync(CreateGuestRequestDto dto);
    Task<ServiceResult<GuestResponseDto>> UpdateAsync(int id, UpdateGuestRequestDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
