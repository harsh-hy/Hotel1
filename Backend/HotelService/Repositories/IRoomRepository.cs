using HotelService.Models;

namespace HotelService.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAllWithTypeAsync();
    Task<Room?> GetByIdWithTypeAsync(int id);
    Task<Room?> GetByIdAsync(int id);
    Task<List<Room>> GetAvailableRoomsAsync(HashSet<int> occupiedIds, int minCapacity);
    Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null);
    Task AddAsync(Room room);
    Task DeleteAsync(Room room);
    Task SaveChangesAsync();
}
