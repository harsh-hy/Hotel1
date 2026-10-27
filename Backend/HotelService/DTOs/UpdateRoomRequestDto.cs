using System.ComponentModel.DataAnnotations;

namespace HotelService.DTOs;

public class UpdateRoomRequestDto
{
    [Required, MaxLength(20)] public string RoomNumber { get; set; } = "";
    public int RoomTypeId { get; set; }
    [Range(0, 9_999_999)] public decimal PricePerNight { get; set; }
    [Required] public string Status { get; set; } = "Available";
    [MaxLength(500)] public string? Description { get; set; }
}
