using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController, Route("api/identity/users"), Authorize(Roles = "Owner")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await userService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await userService.GetByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequestDto dto)
    {
        var result = await userService.CreateAsync(dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.UserId }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequestDto dto)
    {
        var result = await userService.UpdateAsync(id, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await userService.DeleteAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }
}
