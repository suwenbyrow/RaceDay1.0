using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/events")]
[Authorize] // everything here needs a logged-in user; some actions also need the Organiser role
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static EventResponse ToResponse(Event e) =>
        new(e.EventID, e.EventName, e.EventDate, e.Location, e.RouteDescription, e.OrganiserID);

    /// <summary>List all events, soonest first.</summary>
    /// <remarks>Any logged-in user (Organiser or Participant) can view events.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events.OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(ToResponse));
    }

    /// <summary>View a single event's details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e == null) return NotFound(new { message = "Event not found." });
        return Ok(ToResponse(e));
    }

    /// <summary>Create a new event (Organisers only).</summary>
    /// <remarks>The event is automatically linked to the logged-in Organiser. A Participant gets 403.</remarks>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(EventRequest request)
    {
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null) return Forbid();

        var e = new Event
        {
            EventName = request.EventName.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            RouteDescription = request.RouteDescription?.Trim(),
            OrganiserID = organiser.OrganiserID
        };
        _db.Events.Add(e);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = e.EventID }, ToResponse(e));
    }

    /// <summary>Update an event (the Organiser who created it only).</summary>
    /// <remarks>Another Organiser, or a Participant, gets 403.</remarks>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var e = await _db.Events.Include(x => x.Organiser).FirstOrDefaultAsync(x => x.EventID == id);
        if (e == null) return NotFound(new { message = "Event not found." });
        if (e.Organiser!.UserID != CurrentUserId) return Forbid();

        e.EventName = request.EventName.Trim();
        e.EventDate = request.EventDate;
        e.Location = request.Location.Trim();
        e.RouteDescription = request.RouteDescription?.Trim();
        await _db.SaveChangesAsync();

        return Ok(ToResponse(e));
    }

    /// <summary>Delete an event (the Organiser who created it only).</summary>
    /// <remarks>An event that already has categories or enrolments cannot be deleted (409).</remarks>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Events.Include(x => x.Organiser).FirstOrDefaultAsync(x => x.EventID == id);
        if (e == null) return NotFound(new { message = "Event not found." });
        if (e.Organiser!.UserID != CurrentUserId) return Forbid();

        if (await _db.Categories.AnyAsync(c => c.EventID == id) ||
            await _db.Enrolments.AnyAsync(en => en.EventID == id))
            return Conflict(new { message = "This event has categories or enrolments and cannot be deleted." });

        _db.Events.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}