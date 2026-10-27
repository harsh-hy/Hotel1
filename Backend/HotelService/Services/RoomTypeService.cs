using HotelService.DTOs;
using HotelService.Models;
using HotelService.Repositories;

namespace HotelService.Services;

public class RoomTypeService(IRoomTypeRepository repo) : IRoomTypeService
{
    public async Task<ServiceResult<List<RoomTypeResponseDto>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return ServiceResult<List<RoomTypeResponseDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ServiceResult<RoomTypeResponseDto>> GetByIdAsync(int id)
    {
        var rt = await repo.GetByIdAsync(id);
        return rt is null
            ? ServiceResult<RoomTypeResponseDto>.Fail("Room type not found.", 404)
            : ServiceResult<RoomTypeResponseDto>.Ok(Map(rt));
    }

    public async Task<ServiceResult<RoomTypeResponseDto>> CreateAsync(CreateRoomTypeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TypeName))
            return ServiceResult<RoomTypeResponseDto>.Fail("Type name is required.", 400);
        if (dto.Capacity < 1)
            return ServiceResult<RoomTypeResponseDto>.Fail("Capacity must be at least 1.", 400);
        if (await repo.NameExistsAsync(dto.TypeName))
            return ServiceResult<RoomTypeResponseDto>.Fail("Room type name already exists.", 409);

        var rt = new RoomType
        {
            TypeName = dto.TypeName.Trim(),
            Capacity = dto.Capacity,
            Description = dto.Description
        };
        await repo.AddAsync(rt);
        await repo.SaveChangesAsync();
        return ServiceResult<RoomTypeResponseDto>.Ok(Map(rt));
    }

    public async Task<ServiceResult<RoomTypeResponseDto>> UpdateAsync(int id, UpdateRoomTypeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TypeName))
            return ServiceResult<RoomTypeResponseDto>.Fail("Type name is required.", 400);
        if (dto.Capacity < 1)
            return ServiceResult<RoomTypeResponseDto>.Fail("Capacity must be at least 1.", 400);

        var rt = await repo.GetByIdAsync(id);
        if (rt is null) return ServiceResult<RoomTypeResponseDto>.Fail("Room type not found.", 404);

        if (await repo.NameExistsAsync(dto.TypeName, excludeId: id))
            return ServiceResult<RoomTypeResponseDto>.Fail("Room type name already exists.", 409);

        rt.TypeName = dto.TypeName.Trim();
        rt.Capacity = dto.Capacity;
        rt.Description = dto.Description;
        await repo.SaveChangesAsync();
        return ServiceResult<RoomTypeResponseDto>.Ok(Map(rt));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var rt = await repo.GetByIdAsync(id);
        if (rt is null) return ServiceResult<bool>.Fail("Room type not found.", 404);

        if (await repo.HasRoomsAsync(id))
            return ServiceResult<bool>.Fail("Cannot delete room type: rooms are still assigned to it.", 409);

        await repo.DeleteAsync(rt);
        await repo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static RoomTypeResponseDto Map(RoomType t) => new()
    {
        RoomTypeId = t.RoomTypeId,
        TypeName = t.TypeName,
        Capacity = t.Capacity,
        Description = t.Description
    };
}
