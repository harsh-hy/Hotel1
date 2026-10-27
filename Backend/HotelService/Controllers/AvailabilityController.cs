using HotelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelService.Controllers;

[ApiController, Route("api/hotel"), Authorize]
public class AvailabilityController(IAvailabilityService availabilityService) : ControllerBase
{
    // GET /api/hotel/rooms/available?checkIn=2025-01-10&checkOut=2025-01-15&adults=2&children=0
    [HttpGet("rooms/available")]
    public async Task<IActionResult> GetAvailableRooms(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int adults = 1,
        [FromQuery] int children = 0)
    {
        var result = await availabilityService.GetAvailableRoomsAsync(checkIn, checkOut, adults, children);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }

    // GET /api/hotel/rooms/{id}/availability?checkIn=2025-01-10&checkOut=2025-01-15
    [HttpGet("rooms/{id:int}/availability")]
    public async Task<IActionResult> GetRoomAvailability(
        int id,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut)
    {
        var result = await availabilityService.GetRoomAvailabilityAsync(id, checkIn, checkOut);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }
}
