namespace HotelService.DTOs;

public class RoomTypeResponseDto
{
    public int RoomTypeId { get; set; }
    public string TypeName { get; set; } = "";
    public int Capacity { get; set; }
    public string? Description { get; set; }
}
