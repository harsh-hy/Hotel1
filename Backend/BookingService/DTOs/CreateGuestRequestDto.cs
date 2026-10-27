using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs;

public class CreateGuestRequestDto
{
    [Required, MaxLength(120)] public string FullName { get; set; } = "";
    [Required, MaxLength(30)] public string PhoneNumber { get; set; } = "";
    [EmailAddress, MaxLength(150)] public string? Email { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
}
