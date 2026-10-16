using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Authorize] // everything here needs a logged-in user; changes also need the Organiser role
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static CategoryResponse ToResponse(Category c) =>
        new(c.CategoryID, c.CategoryName, c.EventID);

    /// <summary>List the categories for an event.</summary>
    /// <remarks>Any logged-in user (Organiser or Participant) can view categories.</remarks>
    [HttpGet("api/events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
        return Ok(categories.Select(ToResponse));
    }

    /// <summary>Add a category to an event (the Organiser who owns the event only).</summary>
    /// <remarks>A Participant, or an Organiser who does not own the event, gets 403.</remarks>
    [HttpPost("api/events/{eventId:int}/categories")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.Organiser!.UserID != CurrentUserId) return Forbid();

        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            EventID = eventId
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResponse(category));
    }

    /// <summary>Rename a category (the Organiser who owns the event only).</summary>
    [HttpPut("api/categories/{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var category = await _db.Categories
            .Include(c => c.Event).ThenInclude(e => e!.Organiser)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound(new { message = "Category not found." });
        if (category.Event!.Organiser!.UserID != CurrentUserId) return Forbid();

        category.CategoryName = request.CategoryName.Trim();
        await _db.SaveChangesAsync();

        return Ok(ToResponse(category));
    }

    /// <summary>Delete a category (the Organiser who owns the event only).</summary>
    /// <remarks>A category that already has enrolments cannot be deleted (409).</remarks>
    [HttpDelete("api/categories/{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Event).ThenInclude(e => e!.Organiser)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound(new { message = "Category not found." });
        if (category.Event!.Organiser!.UserID != CurrentUserId) return Forbid();

        if (await _db.Enrolments.AnyAsync(en => en.CategoryID == id))
            return Conflict(new { message = "This category has enrolments and cannot be deleted." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}