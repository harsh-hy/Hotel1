using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class ChangePasswordRequestDto
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(10)] public string NewPassword { get; set; } = "";
}
