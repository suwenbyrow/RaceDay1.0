using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Authorize(Roles = "Participant")] // every endpoint here is Participant-only
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Enrol the logged-in Participant in an event, in a chosen category.</summary>
    /// <remarks>The category must belong to the event. A Participant can only enrol once per event (409 otherwise). Organisers get 403.</remarks>
    [HttpPost("api/events/{eventId:int}/enrolments")]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int eventId, EnrolRequest request)
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant == null) return Forbid();

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == request.CategoryID && c.EventID == eventId);
        if (category == null)
            return BadRequest(new { message = "That category does not belong to this event." });

        if (await _db.Enrolments.AnyAsync(e => e.ParticipantID == participant.ParticipantID && e.EventID == eventId))
            return Conflict(new { message = "You are already enrolled in this event." });

        var enrolment = new Enrolment
        {
            ParticipantID = participant.ParticipantID,
            EventID = eventId,
            CategoryID = category.CategoryID,
            EnrolmentDate = DateOnly.FromDateTime(DateTime.Today)
        };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new EnrolmentResponse(
            enrolment.EnrolmentID, ev.EventID, ev.EventName, ev.EventDate,
            category.CategoryID, category.CategoryName, enrolment.EnrolmentDate));
    }

    /// <summary>View the logged-in Participant's own enrolments.</summary>
    /// <remarks>Returns each event and category the Participant has entered, soonest event first.</remarks>
    [HttpGet("api/users/me/enrolments")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant == null) return Forbid();

        var enrolments = await _db.Enrolments
            .Where(e => e.ParticipantID == participant.ParticipantID)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .OrderBy(e => e.Event!.EventDate)
            .ToListAsync();

        return Ok(enrolments.Select(e => new EnrolmentResponse(
            e.EnrolmentID, e.EventID, e.Event!.EventName, e.Event.EventDate,
            e.CategoryID, e.Category!.CategoryName, e.EnrolmentDate)));
    }

    /// <summary>Cancel the logged-in Participant's own enrolment.</summary>
    /// <remarks>Someone else's enrolment gets 403. An enrolment that already has a result cannot be cancelled (409).</remarks>
    [HttpDelete("api/enrolments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Participant)
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Participant!.UserID != CurrentUserId) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == id))
            return Conflict(new { message = "This enrolment already has a result and cannot be cancelled." });

        _db.Enrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}