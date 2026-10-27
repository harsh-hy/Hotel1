using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Models;

public class Payment
{
    public int PaymentId { get; set; }

    public int ReservationId { get; set; }

    [ForeignKey(nameof(ReservationId))]
    public Reservation? Reservation { get; set; }

    public decimal Amount { get; set; }

    [Required, MaxLength(10)]
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, UPI

    [Required, MaxLength(10)]
    public string PaymentStatus { get; set; } = "Paid";

    [Required, MaxLength(80)]
    public string TransactionReference { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
