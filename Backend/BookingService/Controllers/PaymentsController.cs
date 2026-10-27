using BookingService.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController, Route("api/booking"), Authorize]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost("payments/mock")]
    public async Task<IActionResult> MockPayment(MockPaymentRequestDto dto)
    {
        var result = await paymentService.RecordPaymentAsync(dto);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Created($"/api/booking/payments/{result.Data!.PaymentId}", result.Data);
    }

    [HttpGet("payments/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await paymentService.GetByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpGet("reservations/{id:int}/payments")]
    public async Task<IActionResult> GetByReservation(int id)
    {
        var result = await paymentService.GetByReservationIdAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Error });
        return Ok(result.Data);
    }
}
