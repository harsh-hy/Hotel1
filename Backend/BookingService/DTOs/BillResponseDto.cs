namespace BookingService.DTOs;

public class BillGuestDto
{
    public int GuestId { get; set; }
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class BillRoomDto
{
    public int RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomTypeName { get; set; }
    public int? RoomTypeCapacity { get; set; }
}

public class BillPaymentDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string PaymentStatus { get; set; } = "";
    public string TransactionReference { get; set; } = "";
    public DateTime PaymentDate { get; set; }
}

public class BillResponseDto
{
    public string BillReference { get; set; } = "";
    public int ReservationId { get; set; }
    public BillGuestDto Guest { get; set; } = null!;
    public BillRoomDto Room { get; set; } = null!;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int Nights { get; set; }
    public decimal PricePerNightSnapshot { get; set; }
    public decimal TotalRoomCharges { get; set; }
    public decimal PaymentsReceived { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string PaymentStatus { get; set; } = "";
    public List<BillPaymentDto> Payments { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}
