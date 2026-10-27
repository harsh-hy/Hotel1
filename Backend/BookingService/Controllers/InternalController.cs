using BookingService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Controllers;

/// <summary>
/// Internal endpoints — called by HotelService only (not exposed publicly via Ocelot).
/// HotelService asks: which rooms are occupied in a date range?
/// </summary>
[ApiController, Route("api/booking/internal")]
public class InternalController(BookingDbContext db) : ControllerBase
{
    // Returns list of RoomIds that have non-cancelled reservations overlapping [checkIn, checkOut)
    [HttpGet("occupied-rooms")]
    public async Task<IActionResult> OccupiedRooms(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut)
    {
        if (checkOut <= checkIn)
            return BadRequest(new { message = "checkOut must be after checkIn." });

        var ids = await db.Reservations
            .Where(r =>
                r.ReservationStatus != "Cancelled" &&
                r.CheckInDate < checkOut &&
                r.CheckOutDate > checkIn)
            .Select(r => r.RoomId)
            .Distinct()
            .ToListAsync();

        return Ok(ids);
    }

    // Returns true if a room has any non-cancelled reservations
    [HttpGet("rooms/{roomId:int}/has-active-reservations")]
    public async Task<IActionResult> HasActiveReservations(int roomId)
    {
        var has = await db.Reservations
            .AnyAsync(r => r.RoomId == roomId && r.ReservationStatus != "Cancelled");
        return Ok(has);
    }
}
