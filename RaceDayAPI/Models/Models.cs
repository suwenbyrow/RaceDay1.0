namespace RaceDayAPI.Models;

// USERS: base login table for everyone
public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ORGANISERS: a User who can create and manage events
public class Organiser
{
    public int OrganiserID { get; set; }
    public int UserID { get; set; }
    public User? User { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

// PARTICIPANTS: a User who can enrol in events
public class Participant
{
    public int ParticipantID { get; set; }
    public int UserID { get; set; }
    public User? User { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

// EVENTS: created by an Organiser
public class Event
{
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? RouteDescription { get; set; }
    public int OrganiserID { get; set; }
    public Organiser? Organiser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

// CATEGORIES: belongs to one Event
public class Category
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EventID { get; set; }
    public Event? Event { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

// ENROLMENTS: links a Participant to an Event and Category
public class Enrolment
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public DateOnly EnrolmentDate { get; set; }
    public Participant? Participant { get; set; }
    public Event? Event { get; set; }
    public Category? Category { get; set; }
    public Result? Result { get; set; }
}

// RESULTS: one result per Enrolment
public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string? FinishTime { get; set; }
    public int? Position { get; set; }
    public Enrolment? Enrolment { get; set; }
}