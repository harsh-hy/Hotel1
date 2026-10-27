using System.Security.Claims;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController, Route("api/identity")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return result.Success ? Ok(result.Data) : Unauthorized(new { message = result.Error });
    }

    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return Unauthorized();
        var result = await authService.GetMeAsync(id);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Error });
    }

    [HttpPost("change-password"), Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return Unauthorized();
        var result = await authService.ChangePasswordAsync(id, dto);
        return result.Success
            ? NoContent()
            : StatusCode(result.StatusCode, new { message = result.Error });
    }
}
