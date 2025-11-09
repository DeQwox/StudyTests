using StudyTests.Models.Entities;

namespace StudyTests.Models.DTO.Tests;

public class TestViewModel
{
    public Test Test { get; set; } = null!;
    public PassedTest? Passed { get; set; }
}
