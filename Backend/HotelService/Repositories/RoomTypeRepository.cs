using HotelService.Data;
using HotelService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelService.Repositories;

public class RoomTypeRepository(HotelDbContext db) : IRoomTypeRepository
{
    public Task<List<RoomType>> GetAllAsync() =>
        db.RoomTypes.AsNoTracking().OrderBy(t => t.TypeName).ToListAsync();

    public Task<RoomType?> GetByIdAsync(int id) =>
        db.RoomTypes.FindAsync(id).AsTask();

    public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        db.RoomTypes.AnyAsync(t => t.TypeName == name && (excludeId == null || t.RoomTypeId != excludeId));

    public Task<bool> HasRoomsAsync(int roomTypeId) =>
        db.Rooms.AnyAsync(r => r.RoomTypeId == roomTypeId);

    public async Task AddAsync(RoomType roomType) => await db.RoomTypes.AddAsync(roomType);

    public Task DeleteAsync(RoomType roomType)
    {
        db.RoomTypes.Remove(roomType);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
