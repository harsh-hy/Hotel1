using HotelService.DTOs;
using HotelService.Models;
using HotelService.Repositories;
using System.Net.Http.Json;

namespace HotelService.Services;

public class RoomService(IRoomRepository roomRepo, IRoomTypeRepository roomTypeRepo, IHttpClientFactory httpFactory) : IRoomService
{
    private static readonly string[] ValidStatuses = ["Available", "Occupied", "Maintenance", "Inactive"];

    public async Task<ServiceResult<List<RoomResponseDto>>> GetAllAsync()
    {
        var rooms = await roomRepo.GetAllWithTypeAsync();
        return ServiceResult<List<RoomResponseDto>>.Ok(rooms.Select(Map).ToList());
    }

    public async Task<ServiceResult<RoomResponseDto>> GetByIdAsync(int id)
    {
        var room = await roomRepo.GetByIdWithTypeAsync(id);
        return room is null
            ? ServiceResult<RoomResponseDto>.Fail("Room not found.", 404)
            : ServiceResult<RoomResponseDto>.Ok(Map(room));
    }

    public async Task<ServiceResult<RoomResponseDto>> CreateAsync(CreateRoomRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoomNumber))
            return ServiceResult<RoomResponseDto>.Fail("Room number is required.", 400);
        if (dto.PricePerNight < 0)
            return ServiceResult<RoomResponseDto>.Fail("Price must be non-negative.", 400);
        if (!ValidStatuses.Contains(dto.Status))
            return ServiceResult<RoomResponseDto>.Fail($"Status must be one of: {string.Join(", ", ValidStatuses)}.", 400);

        var roomType = await roomTypeRepo.GetByIdAsync(dto.RoomTypeId);
        if (roomType is null)
            return ServiceResult<RoomResponseDto>.Fail("Room type does not exist.", 400);
        if (await roomRepo.RoomNumberExistsAsync(dto.RoomNumber))
            return ServiceResult<RoomResponseDto>.Fail("Room number already exists.", 409);

        var room = new Room
        {
            RoomNumber = dto.RoomNumber.Trim(),
            RoomTypeId = dto.RoomTypeId,
            PricePerNight = dto.PricePerNight,
            Status = dto.Status,
            Description = dto.Description,
            RoomType = roomType
        };
        await roomRepo.AddAsync(room);
        await roomRepo.SaveChangesAsync();
        return ServiceResult<RoomResponseDto>.Ok(Map(room));
    }

    public async Task<ServiceResult<RoomResponseDto>> UpdateAsync(int id, UpdateRoomRequestDto dto)
    {
        if (dto.PricePerNight < 0)
            return ServiceResult<RoomResponseDto>.Fail("Price must be non-negative.", 400);
        if (!ValidStatuses.Contains(dto.Status))
            return ServiceResult<RoomResponseDto>.Fail($"Status must be one of: {string.Join(", ", ValidStatuses)}.", 400);

        var room = await roomRepo.GetByIdWithTypeAsync(id);
        if (room is null) return ServiceResult<RoomResponseDto>.Fail("Room not found.", 404);

        var roomType = await roomTypeRepo.GetByIdAsync(dto.RoomTypeId);
        if (roomType is null)
            return ServiceResult<RoomResponseDto>.Fail("Room type does not exist.", 400);
        if (await roomRepo.RoomNumberExistsAsync(dto.RoomNumber, excludeId: id))
            return ServiceResult<RoomResponseDto>.Fail("Room number already exists.", 409);

        // GetByIdWithTypeAsync uses AsNoTracking, so we need the tracked entity to update
        var trackedRoom = await roomRepo.GetByIdAsync(id);
        trackedRoom!.RoomNumber = dto.RoomNumber.Trim();
        trackedRoom.RoomTypeId = dto.RoomTypeId;
        trackedRoom.PricePerNight = dto.PricePerNight;
        trackedRoom.Status = dto.Status;
        trackedRoom.Description = dto.Description;
        await roomRepo.SaveChangesAsync();

        trackedRoom.RoomType = roomType;
        return ServiceResult<RoomResponseDto>.Ok(Map(trackedRoom));
    }

    public async Task<ServiceResult<bool>> UpdatePriceAsync(int id, UpdateRoomPriceRequestDto dto)
    {
        if (dto.PricePerNight < 0)
            return ServiceResult<bool>.Fail("Price must be non-negative.", 400);

        var room = await roomRepo.GetByIdAsync(id);
        if (room is null) return ServiceResult<bool>.Fail("Room not found.", 404);

        room.PricePerNight = dto.PricePerNight;
        await roomRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, string? bookingServiceBaseUrl)
    {
        var room = await roomRepo.GetByIdAsync(id);
        if (room is null) return ServiceResult<bool>.Fail("Room not found.", 404);

        // Check with BookingService for active reservations
        try
        {
            var client = httpFactory.CreateClient("BookingService");
            var resp = await client.GetAsync($"/api/booking/internal/rooms/{id}/has-active-reservations");
            if (resp.IsSuccessStatusCode)
            {
                var hasActive = await resp.Content.ReadFromJsonAsync<bool>();
                if (hasActive)
                    return ServiceResult<bool>.Fail("Room cannot be deleted: it has active reservations.", 409);
            }
        }
        catch
        {
            // BookingService unavailable: soft-deactivate as safe fallback
            room.Status = "Inactive";
            await roomRepo.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        await roomRepo.DeleteAsync(room);
        await roomRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static RoomResponseDto Map(Room r) => new()
    {
        RoomId = r.RoomId,
        RoomNumber = r.RoomNumber,
        RoomTypeId = r.RoomTypeId,
        RoomTypeName = r.RoomType?.TypeName,
        RoomTypeCapacity = r.RoomType?.Capacity,
        PricePerNight = r.PricePerNight,
        Status = r.Status,
        Description = r.Description
    };
}
