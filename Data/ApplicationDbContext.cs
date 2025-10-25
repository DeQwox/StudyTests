using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyTests.Models.Entities;

namespace StudyTests.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<PassedTest> PassedTests { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<Question> Questions { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store List<string> Answers as JSON in a text column
        var answersConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<string>, string>(
            v => Data.JsonHelpers.SerializeAnswers(v),
            v => Data.JsonHelpers.DeserializeAnswers(v));

        modelBuilder.Entity<Question>()
            .Property(q => q.Answers)
            .HasConversion(answersConverter)
            .HasColumnType("TEXT");

        // Ensure Questions collection is initialized when creating Test/Question relationships
        modelBuilder.Entity<Test>()
            .HasMany(t => t.Questions)
            .WithOne(q => q.Test)
            .HasForeignKey(q => q.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}