using IdentityService.Models;

namespace IdentityService.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
    Task<bool> AnyOwnerExistsAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
