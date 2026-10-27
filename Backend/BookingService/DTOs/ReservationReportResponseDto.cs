namespace BookingService.DTOs;

public class ReservationReportItemDto
{
    public int ReservationId { get; set; }
    public string? GuestName { get; set; }
    public int RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int Nights { get; set; }
    public decimal PricePerNightSnapshot { get; set; }
    public decimal TotalAmount { get; set; }
    public string ReservationStatus { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ReservationStatusCountDto
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
}

public class ReservationReportResponseDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public string? StatusFilter { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ReservationStatusCountDto> ByStatus { get; set; } = [];
    public List<ReservationReportItemDto> Reservations { get; set; } = [];
}
