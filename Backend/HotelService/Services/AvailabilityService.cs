using HotelService.DTOs;
using HotelService.Repositories;
using System.Net.Http.Json;

namespace HotelService.Services;

public class AvailabilityService(IRoomRepository roomRepo, IHttpClientFactory httpFactory) : IAvailabilityService
{
    public async Task<ServiceResult<List<RoomResponseDto>>> GetAvailableRoomsAsync(
        DateOnly checkIn, DateOnly checkOut, int adults, int children)
    {
        if (checkOut <= checkIn)
            return ServiceResult<List<RoomResponseDto>>.Fail("checkOut must be after checkIn.", 400);
        if (adults < 1)
            return ServiceResult<List<RoomResponseDto>>.Fail("At least 1 adult required.", 400);
        if (children < 0)
            return ServiceResult<List<RoomResponseDto>>.Fail("Children cannot be negative.", 400);

        var occupiedIds = await GetOccupiedRoomIdsAsync(checkIn, checkOut);
        var totalGuests = adults + children;
        var rooms = await roomRepo.GetAvailableRoomsAsync(occupiedIds, totalGuests);

        return ServiceResult<List<RoomResponseDto>>.Ok(rooms.Select(r => new RoomResponseDto
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            RoomTypeId = r.RoomTypeId,
            RoomTypeName = r.RoomType?.TypeName,
            RoomTypeCapacity = r.RoomType?.Capacity,
            PricePerNight = r.PricePerNight,
            Status = r.Status,
            Description = r.Description
        }).ToList());
    }

    public async Task<ServiceResult<RoomAvailabilityResponseDto>> GetRoomAvailabilityAsync(
        int roomId, DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn)
            return ServiceResult<RoomAvailabilityResponseDto>.Fail("checkOut must be after checkIn.", 400);

        var room = await roomRepo.GetByIdWithTypeAsync(roomId);
        if (room is null)
            return ServiceResult<RoomAvailabilityResponseDto>.Fail("Room not found.", 404);

        var occupiedIds = await GetOccupiedRoomIdsAsync(checkIn, checkOut);
        var isAvailable = room.Status == "Available" && !occupiedIds.Contains(roomId);

        return ServiceResult<RoomAvailabilityResponseDto>.Ok(new RoomAvailabilityResponseDto
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            Status = room.Status,
            CheckIn = checkIn,
            CheckOut = checkOut,
            IsAvailable = isAvailable
        });
    }

    private async Task<HashSet<int>> GetOccupiedRoomIdsAsync(DateOnly checkIn, DateOnly checkOut)
    {
        try
        {
            var client = httpFactory.CreateClient("BookingService");
            var url = $"/api/booking/internal/occupied-rooms?checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}";
            var result = await client.GetFromJsonAsync<List<int>>(url);
            return result?.ToHashSet() ?? [];
        }
        catch
        {
            return [];
        }
    }
}
