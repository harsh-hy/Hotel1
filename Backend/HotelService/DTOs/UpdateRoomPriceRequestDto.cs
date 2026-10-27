using System.ComponentModel.DataAnnotations;

namespace HotelService.DTOs;

public class UpdateRoomPriceRequestDto
{
    [Range(0, 9_999_999)] public decimal PricePerNight { get; set; }
}
