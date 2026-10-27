using HotelService.Models;

namespace HotelService.Repositories;

public interface IRoomTypeRepository
{
    Task<List<RoomType>> GetAllAsync();
    Task<RoomType?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> HasRoomsAsync(int roomTypeId);
    Task AddAsync(RoomType roomType);
    Task DeleteAsync(RoomType roomType);
    Task SaveChangesAsync();
}
