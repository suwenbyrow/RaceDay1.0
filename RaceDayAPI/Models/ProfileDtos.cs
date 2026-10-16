using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models;

/// <summary>The logged-in user's profile. Never includes the password.</summary>
public record ProfileResponse(int UserID, string FirstName, string LastName, string Email, string Role);

/// <summary>Fields a user can change on their own profile.</summary>
public class UpdateProfileRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;
}