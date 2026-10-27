using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models;

public class User
{
    public int UserId { get; set; }

    [Required, MaxLength(80)]
    public string Username { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(120)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Receptionist";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
