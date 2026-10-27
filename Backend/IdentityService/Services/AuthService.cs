using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.DTOs;
using IdentityService.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services;

public class AuthService(IUserRepository userRepo, IConfiguration config) : IAuthService
{
    private readonly PasswordHasher<object> _hasher = new();

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await userRepo.GetByUsernameAsync(dto.Username);
        if (user is null || !user.IsActive)
            return ServiceResult<LoginResponseDto>.Fail("Invalid credentials.", 401);

        var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            return ServiceResult<LoginResponseDto>.Fail("Invalid credentials.", 401);

        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured.");
        var expiry = DateTime.UtcNow.AddMinutes(
            int.TryParse(config["Jwt:ExpiryMinutes"], out var m) ? m : 60);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        var userDto = MapUser(user);
        return ServiceResult<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiry,
            User = userDto
        });
    }

    public async Task<ServiceResult<UserResponseDto>> GetMeAsync(int userId)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null)
            return ServiceResult<UserResponseDto>.Fail("User not found.", 404);
        return ServiceResult<UserResponseDto>.Ok(MapUser(user));
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto dto)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null)
            return ServiceResult<bool>.Fail("User not found.", 404);

        if (_hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.CurrentPassword)
            == PasswordVerificationResult.Failed)
            return ServiceResult<bool>.Fail("Current password is incorrect.", 400);

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 10)
            return ServiceResult<bool>.Fail("New password must be at least 10 characters.", 400);

        user.PasswordHash = _hasher.HashPassword(new object(), dto.NewPassword);
        await userRepo.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static UserResponseDto MapUser(IdentityService.Models.User u) => new()
    {
        UserId = u.UserId,
        Username = u.Username,
        FullName = u.FullName,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
