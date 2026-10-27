using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class UpdateUserRequestDto
{
    [Required, MaxLength(120)] public string FullName { get; set; } = "";
    [Required] public string Role { get; set; } = "";  // Owner, Manager, or Receptionist
    public bool IsActive { get; set; }
}
