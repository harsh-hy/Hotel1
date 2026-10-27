using System.ComponentModel.DataAnnotations;

namespace HotelService.Models;

public class RoomType
{
    public int RoomTypeId { get; set; }

    [Required, MaxLength(80)]
    public string TypeName { get; set; } = "";

    [Range(1, 50)]
    public int Capacity { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<Room> Rooms { get; set; } = [];
}
