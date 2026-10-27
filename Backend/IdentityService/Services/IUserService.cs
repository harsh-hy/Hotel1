using IdentityService.DTOs;

namespace IdentityService.Services;

public interface IUserService
{
    Task<ServiceResult<List<UserResponseDto>>> GetAllAsync();
    Task<ServiceResult<UserResponseDto>> GetByIdAsync(int id);
    Task<ServiceResult<UserResponseDto>> CreateAsync(CreateUserRequestDto dto);
    Task<ServiceResult<UserResponseDto>> UpdateAsync(int id, UpdateUserRequestDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
