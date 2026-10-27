namespace BookingService.HttpClients;

/// <summary>
/// Represents room data fetched from HotelService via HTTP.
/// </summary>
public class HotelRoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = "";
    public int RoomTypeId { get; set; }
    public string? RoomTypeName { get; set; }
    public int RoomTypeCapacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = "";
    public string? Description { get; set; }
}
