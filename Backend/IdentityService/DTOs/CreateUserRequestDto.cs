using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class CreateUserRequestDto
{
    [Required, MaxLength(80)] public string Username { get; set; } = "";
    [Required, MinLength(10)] public string Password { get; set; } = "";
    [Required, MaxLength(120)] public string FullName { get; set; } = "";
    [Required] public string Role { get; set; } = ""; // Manager or Receptionist only
}
