using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models;

/// <summary>The category a Participant wants to enter for an event.</summary>
public class EnrolRequest
{
    [Required, Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}

/// <summary>An enrolment as returned by the API.</summary>
public record EnrolmentResponse(
    int EnrolmentID,
    int EventID,
    string EventName,
    DateOnly EventDate,
    int CategoryID,
    string CategoryName,
    DateOnly EnrolmentDate);