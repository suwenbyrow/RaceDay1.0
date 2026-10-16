using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models;

/// <summary>A finish time and position an Organiser records for an enrolment.</summary>
public class ResultRequest
{
    /// <summary>Format: hh:mm:ss, e.g. 01:52:30</summary>
    [Required, RegularExpression(@"^\d{2}:[0-5]\d:[0-5]\d$",
        ErrorMessage = "FinishTime must be in the format hh:mm:ss, e.g. 01:52:30.")]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Position must be 1 or higher.")]
    public int Position { get; set; }
}

/// <summary>A result as shown in an event's results list.</summary>
public record ResultResponse(
    int ResultID,
    int EnrolmentID,
    string ParticipantName,
    string CategoryName,
    string? FinishTime,
    int? Position);

/// <summary>A result as shown in a Participant's own race history.</summary>
public record MyResultResponse(
    int ResultID,
    int EventID,
    string EventName,
    DateOnly EventDate,
    string CategoryName,
    string? FinishTime,
    int? Position,
    int TotalFinishers);