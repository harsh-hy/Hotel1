using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController, Route("api/booking/reservations/{id:int}/bill"), Authorize]
public class BillsController(IBillService billService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBill(int id)
    {
        var jwt = Request.Headers.Authorization.ToString() is { Length: > 0 } v ? v : null;
        var result = await billService.GetBillAsync(id, jwt);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }
}
