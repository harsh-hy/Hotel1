using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Models;

public class Reservation
{
    public int ReservationId { get; set; }

    public int GuestId { get; set; }

    [ForeignKey(nameof(GuestId))]
    public Guest? Guest { get; set; }

    /// <summary>
    /// Logical cross-service reference to HotelDB.Rooms.
    /// Not a SQL foreign key — no EF navigation property across databases.
    /// </summary>
    public int RoomId { get; set; }

    [Required]
    public DateOnly CheckInDate { get; set; }

    [Required]
    public DateOnly CheckOutDate { get; set; }

    [Range(1, 50)]
    public int NumberOfAdults { get; set; }

    [Range(0, 50)]
    public int NumberOfChildren { get; set; }

    public decimal PricePerNightSnapshot { get; set; }
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(20)]
    public string ReservationStatus { get; set; } = "Booked";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<Payment> Payments { get; set; } = [];
}
