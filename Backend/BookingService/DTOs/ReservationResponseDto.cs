namespace BookingService.DTOs;

public class ReservationResponseDto
{
    public int ReservationId { get; set; }
    public int GuestId { get; set; }
    public string? GuestName { get; set; }
    public int RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int Nights { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public decimal PricePerNightSnapshot { get; set; }
    public decimal TotalAmount { get; set; }
    public string ReservationStatus { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
