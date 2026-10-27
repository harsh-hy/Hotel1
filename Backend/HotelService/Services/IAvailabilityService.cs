using HotelService.DTOs;

namespace HotelService.Services;

public interface IAvailabilityService
{
    Task<ServiceResult<List<RoomResponseDto>>> GetAvailableRoomsAsync(DateOnly checkIn, DateOnly checkOut, int adults, int children);
    Task<ServiceResult<RoomAvailabilityResponseDto>> GetRoomAvailabilityAsync(int roomId, DateOnly checkIn, DateOnly checkOut);
}
