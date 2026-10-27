using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.Controllers;

[ApiController, Route("api/booking/dashboard"), Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await dashboardService.GetDashboardAsync(role);
        return Ok(result.Data);
    }
}
