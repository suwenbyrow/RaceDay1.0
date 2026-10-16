using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Authorize] // everything here needs a logged-in user; some actions also need a specific role
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static ResultResponse ToResponse(Result r) =>
        new(r.ResultID, r.EnrolmentID,
            $"{r.Enrolment!.Participant!.User!.FirstName} {r.Enrolment.Participant.User.LastName}",
            r.Enrolment.Category!.CategoryName, r.FinishTime, r.Position);

    /// <summary>Record a result for an enrolment (the Organiser who owns the event only).</summary>
    /// <remarks>FinishTime must be hh:mm:ss and Position 1 or higher. An enrolment can only have one result (409). A Participant, or an Organiser who does not own the event, gets 403.</remarks>
    [HttpPost("api/enrolments/{enrolmentId:int}/results")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Record(int enrolmentId, ResultRequest request)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Event).ThenInclude(ev => ev!.Organiser)
            .Include(e => e.Participant).ThenInclude(p => p!.User)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);
        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Event!.Organiser!.UserID != CurrentUserId) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == enrolmentId))
            return Conflict(new { message = "This enrolment already has a result." });

        var result = new Result
        {
            EnrolmentID = enrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position,
            Enrolment = enrolment
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResponse(result));
    }

    /// <summary>View all results for an event, best position first.</summary>
    /// <remarks>Any logged-in user (Organiser or Participant) can view an event's results.</remarks>
    [HttpGet("api/events/{eventId:int}/results")]
    [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var results = await _db.Results
            .Where(r => r.Enrolment!.EventID == eventId)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant).ThenInclude(p => p!.User)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .OrderBy(r => r.Position)
            .ToListAsync();

        return Ok(results.Select(ToResponse));
    }

    /// <summary>View the logged-in Participant's own race history.</summary>
    /// <remarks>Shows each event, category, finish time and position, plus how many people finished that event. Organisers get 403.</remarks>
    [HttpGet("api/users/me/results")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(IEnumerable<MyResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant == null) return Forbid();

        var results = await _db.Results
            .Where(r => r.Enrolment!.ParticipantID == participant.ParticipantID)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
            .OrderByDescending(r => r.Enrolment!.Event!.EventDate)
            .ToListAsync();

        // How many people finished each of these events (for "3rd out of 12").
        var eventIds = results.Select(r => r.Enrolment!.EventID).Distinct().ToList();
        var totals = await _db.Results
            .Where(r => eventIds.Contains(r.Enrolment!.EventID))
            .GroupBy(r => r.Enrolment!.EventID)
            .Select(g => new { EventID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventID, x => x.Count);

        return Ok(results.Select(r => new MyResultResponse(
            r.ResultID, r.Enrolment!.EventID, r.Enrolment.Event!.EventName, r.Enrolment.Event.EventDate,
            r.Enrolment.Category!.CategoryName, r.FinishTime, r.Position,
            totals[r.Enrolment.EventID])));
    }
}