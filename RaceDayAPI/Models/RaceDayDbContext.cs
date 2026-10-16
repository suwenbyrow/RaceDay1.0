using Microsoft.EntityFrameworkCore;

namespace RaceDayAPI.Models;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    // One DbSet per table. The names match the tables in your SQL script.
    public DbSet<User> Users => Set<User>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // In your SQL script, OrganiserID and ParticipantID are NOT auto-numbered
        // (no IDENTITY), so EF Core must not expect the database to generate them.
        modelBuilder.Entity<Organiser>()
            .Property(o => o.OrganiserID)
            .ValueGeneratedNever();

        modelBuilder.Entity<Participant>()
            .Property(p => p.ParticipantID)
            .ValueGeneratedNever();

        // Email must be unique (matches the UNIQUE constraint in the SQL).
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // One Enrolment has at most one Result (matches the UNIQUE on Results.EnrolmentID).
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Result)
            .WithOne(r => r.Enrolment)
            .HasForeignKey<Result>(r => r.EnrolmentID);
    }
}