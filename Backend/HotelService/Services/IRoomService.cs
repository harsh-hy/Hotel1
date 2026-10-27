using HotelService.DTOs;

namespace HotelService.Services;

public interface IRoomService
{
    Task<ServiceResult<List<RoomResponseDto>>> GetAllAsync();
    Task<ServiceResult<RoomResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<RoomResponseDto>> CreateAsync(CreateRoomRequestDto dto);
    Task<ServiceResult<RoomResponseDto>> UpdateAsync(int id, UpdateRoomRequestDto dto);
    Task<ServiceResult<bool>> UpdatePriceAsync(int id, UpdateRoomPriceRequestDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id, string? bookingServiceBaseUrl);
}
