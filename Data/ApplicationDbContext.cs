using Microsoft.EntityFrameworkCore;
using StudyTests.Models.Entities;

namespace Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<PassedTest> PassedTests { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<Question> Questions { get; set; }
}