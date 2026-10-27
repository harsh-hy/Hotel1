using System.ComponentModel.DataAnnotations;

namespace HotelService.DTOs;

public class UpdateRoomTypeRequestDto
{
    [Required, MaxLength(80)] public string TypeName { get; set; } = "";
    [Range(1, 50)] public int Capacity { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
}
