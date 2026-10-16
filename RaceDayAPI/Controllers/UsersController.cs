using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // every endpoint here requires a logged-in user (either role)
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    // The user's ID and role come from the session cookie created at login.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role)!;

    /// <summary>View the logged-in user's own profile.</summary>
    /// <remarks>Requires a session (log in first). Returns the user's name, email and role.</remarks>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        return Ok(new ProfileResponse(user.UserID, user.FirstName, user.LastName, user.Email, CurrentRole));
    }

    /// <summary>Update the logged-in user's own profile.</summary>
    /// <remarks>Send FirstName, LastName and Email. The email must not already belong to another account.</remarks>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();

        // The email must stay unique across all users (except this one).
        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserID != user.UserID))
            return Conflict(new { message = "Another account already uses this email." });

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        await _db.SaveChangesAsync();

        return Ok(new ProfileResponse(user.UserID, user.FirstName, user.LastName, user.Email, CurrentRole));
    }
}