namespace HotelService.DTOs;

public class RoomAvailabilityResponseDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public bool IsAvailable { get; set; }
}
