using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyTests.Models.Entities;

public class Question: Entity
{
    public int TestId { get; set; }
    public List<string> Answers { get; set; } = new List<string>();
    public int CorrectAnswerIndex { get; set; }
    public double Score { get; set; }

    [ForeignKey(nameof(TestId))]
    public Test Test { get; set; } = null!;
}