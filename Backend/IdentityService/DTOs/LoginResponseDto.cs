namespace IdentityService.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = null!;
}
