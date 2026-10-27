using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs;

public class UpdateReservationRequestDto
{
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    [Range(1, 50)] public int NumberOfAdults { get; set; }
    [Range(0, 50)] public int NumberOfChildren { get; set; }
}
