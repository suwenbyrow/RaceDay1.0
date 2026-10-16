using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models;

/// <summary>Data needed to create a new account.</summary>
public class RegisterRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, RegularExpression("^(Organiser|Participant)$",
        ErrorMessage = "Role must be 'Organiser' or 'Participant'.")]
    public string Role { get; set; } = string.Empty;
}

/// <summary>Data needed to log in.</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Returned after a successful registration or login. Never includes the password.</summary>
public record AuthResponse(int UserID, string FirstName, string LastName, string Email, string Role);