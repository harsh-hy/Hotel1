using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public class UserService(IUserRepository userRepo) : IUserService
{
    private readonly PasswordHasher<object> _hasher = new();

    public async Task<ServiceResult<List<UserResponseDto>>> GetAllAsync()
    {
        var users = await userRepo.GetAllAsync();
        return ServiceResult<List<UserResponseDto>>.Ok(users.Select(MapUser).ToList());
    }

    public async Task<ServiceResult<UserResponseDto>> GetByIdAsync(int id)
    {
        var user = await userRepo.GetByIdAsync(id);
        if (user is null) return ServiceResult<UserResponseDto>.Fail("User not found.", 404);
        return ServiceResult<UserResponseDto>.Ok(MapUser(user));
    }

    public async Task<ServiceResult<UserResponseDto>> CreateAsync(CreateUserRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            return ServiceResult<UserResponseDto>.Fail("Username is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResult<UserResponseDto>.Fail("Full name is required.", 400);
        if (dto.Password.Length < 10)
            return ServiceResult<UserResponseDto>.Fail("Password must be at least 10 characters.", 400);
        if (dto.Role is not ("Manager" or "Receptionist"))
            return ServiceResult<UserResponseDto>.Fail("Role must be Manager or Receptionist.", 400);

        if (await userRepo.UsernameExistsAsync(dto.Username))
            return ServiceResult<UserResponseDto>.Fail("Username already exists.", 409);

        var user = new User
        {
            Username = dto.Username.Trim(),
            FullName = dto.FullName.Trim(),
            Role = dto.Role,
            PasswordHash = _hasher.HashPassword(new object(), dto.Password),
            IsActive = true
        };
        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();
        return ServiceResult<UserResponseDto>.Ok(MapUser(user));
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateAsync(int id, UpdateUserRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResult<UserResponseDto>.Fail("Full name is required.", 400);
        if (dto.Role is not ("Owner" or "Manager" or "Receptionist"))
            return ServiceResult<UserResponseDto>.Fail("Invalid role.", 400);

        var user = await userRepo.GetByIdAsync(id);
        if (user is null) return ServiceResult<UserResponseDto>.Fail("User not found.", 404);

        user.FullName = dto.FullName.Trim();
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        await userRepo.SaveChangesAsync();
        return ServiceResult<UserResponseDto>.Ok(MapUser(user));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var user = await userRepo.GetByIdAsync(id);
        if (user is null) return ServiceResult<bool>.Fail("User not found.", 404);

        // Soft-delete: deactivate rather than hard-delete to preserve audit trail
        user.IsActive = false;
        await userRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static UserResponseDto MapUser(User u) => new()
    {
        UserId = u.UserId,
        Username = u.Username,
        FullName = u.FullName,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
