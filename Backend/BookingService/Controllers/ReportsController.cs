using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController, Route("api/booking/reports"), Authorize(Roles = "Owner,Manager")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var result = await reportService.GetRevenueReportAsync(from, to);
        return Ok(result.Data);
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> Reservations(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? status)
    {
        var result = await reportService.GetReservationReportAsync(from, to, status);
        return Ok(result.Data);
    }
}
