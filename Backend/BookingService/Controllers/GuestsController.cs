using BookingService.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController, Route("api/booking/guests"), Authorize]
public class GuestsController(IGuestService guestService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await guestService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await guestService.GetByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGuestRequestDto dto)
    {
        var result = await guestService.CreateAsync(dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.GuestId }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateGuestRequestDto dto)
    {
        var result = await guestService.UpdateAsync(id, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await guestService.DeleteAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }
}
