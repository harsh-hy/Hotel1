using BookingService.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController, Route("api/booking/reservations"), Authorize]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
    private string? Jwt => Request.Headers.Authorization.ToString() is { Length: > 0 } v ? v : null;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await reservationService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("today")]
    public async Task<IActionResult> Today()
    {
        var result = await reservationService.GetTodayAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await reservationService.GetByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequestDto dto)
    {
        var result = await reservationService.CreateAsync(dto, Jwt);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ReservationId }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateReservationRequestDto dto)
    {
        var result = await reservationService.UpdateAsync(id, dto, Jwt);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await reservationService.CancelAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }

    [HttpPost("{id:int}/check-in")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var result = await reservationService.CheckInAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }

    [HttpPost("{id:int}/check-out")]
    public async Task<IActionResult> CheckOut(int id)
    {
        var result = await reservationService.CheckOutAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return NoContent();
    }
}
