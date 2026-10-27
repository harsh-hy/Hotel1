using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories;

public class UserRepository(IdentityDbContext db) : IUserRepository
{
    public Task<List<User>> GetAllAsync() =>
        db.Users.AsNoTracking().OrderBy(u => u.UserId).ToListAsync();

    public Task<User?> GetByIdAsync(int id) =>
        db.Users.FindAsync(id).AsTask();

    public Task<User?> GetByUsernameAsync(string username) =>
        db.Users.SingleOrDefaultAsync(u => u.Username == username);

    public Task<bool> UsernameExistsAsync(string username, int? excludeId = null) =>
        db.Users.AnyAsync(u => u.Username == username && (excludeId == null || u.UserId != excludeId));

    public Task<bool> AnyOwnerExistsAsync() =>
        db.Users.AnyAsync(u => u.Role == "Owner");

    public async Task AddAsync(User user) => await db.Users.AddAsync(user);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
