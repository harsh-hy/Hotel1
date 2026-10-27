using HotelService.DTOs;
using HotelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelService.Controllers;

[ApiController, Route("api/hotel/rooms"), Authorize]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await roomService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await roomService.GetByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost, Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create(CreateRoomRequestDto dto)
    {
        var result = await roomService.CreateAsync(dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.RoomId }, result.Data);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, UpdateRoomRequestDto dto)
    {
        var result = await roomService.UpdateAsync(id, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }

    [HttpPut("{id:int}/price"), Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> UpdatePrice(int id, UpdateRoomPriceRequestDto dto)
    {
        var result = await roomService.UpdatePriceAsync(id, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await roomService.DeleteAsync(id, null);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }
}
