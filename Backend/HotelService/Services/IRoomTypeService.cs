using HotelService.DTOs;

namespace HotelService.Services;

public interface IRoomTypeService
{
    Task<ServiceResult<List<RoomTypeResponseDto>>> GetAllAsync();
    Task<ServiceResult<RoomTypeResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<RoomTypeResponseDto>> CreateAsync(CreateRoomTypeRequestDto dto);
    Task<ServiceResult<RoomTypeResponseDto>> UpdateAsync(int id, UpdateRoomTypeRequestDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
