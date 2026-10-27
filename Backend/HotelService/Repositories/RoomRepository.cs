using HotelService.Data;
using HotelService.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelService.Repositories;

public class RoomRepository(HotelDbContext db) : IRoomRepository
{
    public Task<List<Room>> GetAllWithTypeAsync() =>
        db.Rooms.AsNoTracking()
            .Include(r => r.RoomType)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

    public Task<Room?> GetByIdWithTypeAsync(int id) =>
        db.Rooms.AsNoTracking()
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.RoomId == id);

    public Task<Room?> GetByIdAsync(int id) =>
        db.Rooms.FindAsync(id).AsTask();

    public Task<List<Room>> GetAvailableRoomsAsync(HashSet<int> occupiedIds, int minCapacity) =>
        db.Rooms.AsNoTracking()
            .Include(r => r.RoomType)
            .Where(r =>
                r.Status == "Available" &&
                r.RoomType!.Capacity >= minCapacity &&
                !occupiedIds.Contains(r.RoomId))
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

    public Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null) =>
        db.Rooms.AnyAsync(r => r.RoomNumber == roomNumber && (excludeId == null || r.RoomId != excludeId));

    public async Task AddAsync(Room room) => await db.Rooms.AddAsync(room);

    public Task DeleteAsync(Room room)
    {
        db.Rooms.Remove(room);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
