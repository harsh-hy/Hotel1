using HotelService.DTOs;
using HotelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelService.Controllers;

[ApiController, Route("api/hotel/room-types"), Authorize]
public class RoomTypesController(IRoomTypeService roomTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await roomTypeService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await roomTypeService.GetByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost, Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Create(CreateRoomTypeRequestDto dto)
    {
        var result = await roomTypeService.CreateAsync(dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.RoomTypeId }, result.Data);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Update(int id, UpdateRoomTypeRequestDto dto)
    {
        var result = await roomTypeService.UpdateAsync(id, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await roomTypeService.DeleteAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }
}
