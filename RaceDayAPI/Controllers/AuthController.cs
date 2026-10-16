using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AuthController(RaceDayDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>Register a new Organiser or Participant account.</summary>
    /// <remarks>Role must be "Organiser" or "Participant". Email must be unique. Password must be at least 8 characters.</remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email
        };
        // Passwords are stored as a salted hash, never as plain text.
        user.Password = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(); // the database generates user.UserID here

        // Create the matching Organiser or Participant row (same ID as the user).
        if (request.Role == "Organiser")
            _db.Organisers.Add(new Organiser { OrganiserID = user.UserID, UserID = user.UserID });
        else
            _db.Participants.Add(new Participant { ParticipantID = user.UserID, UserID = user.UserID });
        await _db.SaveChangesAsync();

        var response = new AuthResponse(user.UserID, user.FirstName, user.LastName, user.Email, request.Role);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Log in with email and password. Starts a session (cookie).</summary>
    /// <remarks>On success the response sets a session cookie, which Swagger and browsers send automatically on later requests.</remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message for "no such user" and "wrong password" so we don't reveal which emails exist.
        var invalid = Unauthorized(new { message = "Invalid email or password." });
        if (user == null) return invalid;

        PasswordVerificationResult check;
        try
        {
            check = _hasher.VerifyHashedPassword(user, user.Password, request.Password);
        }
        catch (FormatException)
        {
            // Stored value isn't a valid hash (e.g. plain-text sample data).
            return invalid;
        }
        if (check == PasswordVerificationResult.Failed) return invalid;

        var role = await _db.Organisers.AnyAsync(o => o.UserID == user.UserID)
            ? "Organiser"
            : "Participant";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(new AuthResponse(user.UserID, user.FirstName, user.LastName, user.Email, role));
    }

    /// <summary>Log out and end the current session.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}