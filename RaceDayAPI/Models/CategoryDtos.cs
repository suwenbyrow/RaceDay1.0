using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models;

/// <summary>Data an Organiser sends to create or rename a category.</summary>
public class CategoryRequest
{
    [Required, StringLength(50)]
    public string CategoryName { get; set; } = string.Empty;
}

/// <summary>A category as returned by the API.</summary>
public record CategoryResponse(int CategoryID, string CategoryName, int EventID);