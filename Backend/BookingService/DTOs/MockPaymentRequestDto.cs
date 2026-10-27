using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs;

public class MockPaymentRequestDto
{
    public int ReservationId { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [Required] public string PaymentMethod { get; set; } = ""; // Cash, Card, UPI
}
