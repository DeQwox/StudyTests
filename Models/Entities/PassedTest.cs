using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StudyTests.Models.Entities;

public class PassedTest: Entity
{
    public int StudentId { get; set; }
    public int TestId { get; set; }
    public DateTime PassedAt { get; set; } = DateTime.UtcNow;
    public List<string> Answers { get; set; } = new List<string>();

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [ForeignKey(nameof(TestId))]
    public Test Test { get; set; } = null!;

    public double Score { get; set; } = 0;
}