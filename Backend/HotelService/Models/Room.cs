using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelService.Models;

public class Room
{
    public int RoomId { get; set; }

    [Required, MaxLength(20)]
    public string RoomNumber { get; set; } = "";

    public int RoomTypeId { get; set; }

    [ForeignKey(nameof(RoomTypeId))]
    public RoomType? RoomType { get; set; }

    [Range(0, 9_999_999)]
    public decimal PricePerNight { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Available";

    [MaxLength(500)]
    public string? Description { get; set; }
}
