using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models;

/// <summary>Data an Organiser sends to create or update an event.</summary>
public class EventRequest
{
    [Required, StringLength(100)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>Format: yyyy-MM-dd, e.g. 2026-11-15</summary>
    [Required]
    public DateOnly EventDate { get; set; }

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [StringLength(255)]
    public string? RouteDescription { get; set; }
}

/// <summary>An event as returned by the API.</summary>
public record EventResponse(
    int EventID,
    string EventName,
    DateOnly EventDate,
    string Location,
    string? RouteDescription,
    int OrganiserID);