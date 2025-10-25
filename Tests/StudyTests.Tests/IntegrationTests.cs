using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using StudyTests.Data;
using StudyTests.Models.Entities;
using Repositories;
using Xunit;

namespace StudyTests.Tests;

public class IntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TestingRepository _repository;

    public IntegrationTests()
    {
        var a = new DbContextOptionsBuilder<ApplicationDbContext>();
        var options = a.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;

        _context = new ApplicationDbContext(options);
        _repository = new TestingRepository(_context);
        
        SeedTestData();
    }

    [Fact]
    public async Task GetAllTestsAsync_ShouldReturnAllTests()
    {
        // Act
        var tests = await _repository.GetAllTestsAsync();

        // Assert;
        tests.Should().Contain(t => t.Name == "Mathematics Test");
        tests.Should().Contain(t => t.Name == "History Test");
    }

    [Fact]
    public async Task GetTestByIdAsync_WithValidId_ShouldReturnTest()
    {
        // Act
        var test = await _repository.GetTestByIdAsync(1);

        // Assert
        test.Should().NotBeNull();
        test!.Name.Should().Be("Mathematics Test");
        test.Description.Should().Be("Basic mathematics test");
    }

    [Fact]
    public async Task GetTestByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var test = await _repository.GetTestByIdAsync(999);

        // Assert
        test.Should().BeNull();
    }

    [Fact]
    public async Task GetStudentByIdAsync_WithValidId_ShouldReturnStudent()
    {
        // Act
        var student = await _repository.GetStudentByIdAsync(1);

        // Assert
        student.Should().NotBeNull();
        student!.FullName.Should().Be("John Doe");
        student.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetStudentByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var student = await _repository.GetStudentByIdAsync(999);

        // Assert
        student.Should().BeNull();
    }

    [Fact]
    public async Task GetTeacherByIdAsync_WithValidId_ShouldReturnTeacher()
    {
        // Act
        var teacher = await _repository.GetTeacherByIdAsync(1);

        // Assert
        teacher.Should().NotBeNull();
        teacher!.FullName.Should().Be("Jane Smith");
        teacher.Email.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public void GetTestById_WithValidId_ShouldReturnTest()
    {
        // Act
        var test = _repository.GetTestById(1);

        // Assert
        test.Should().NotBeNull();
        test!.Name.Should().Be("Mathematics Test");
    }

    [Fact]
    public void GetTestById_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var test = _repository.GetTestById(999);

        // Assert
        test.Should().BeNull();
    }

    [Fact]
    public void GetTestQuestionList_WithValidTestId_ShouldReturnQuestions()
    {
        // Act
        var questions = _repository.GetTestQuestionList(1).ToList();

        // Assert
        questions.Should().Contain(q => q.Description == "What is 2 + 2?");
        questions.Should().Contain(q => q.Description == "What is 5 * 3?");
        questions.Should().Contain(q => q.Description == "What is 10 / 2?");
    }

    [Fact]
    public void GetTestQuestionList_WithInvalidTestId_ShouldReturnEmptyList()
    {
        // Act
        var questions = _repository.GetTestQuestionList(999).ToList();

        // Assert
        questions.Should().BeEmpty();
    }

    private void SeedTestData()
    {
        // Додаємо вчителя
        var teacher = new Teacher
        {
            Login = "janesmith",
            FullName = "Jane Smith",
            Email = "jane.smith@example.com"
        };
        _context.Teachers.Add(teacher);

        // Додаємо студента
        var student = new Student
        {
            Login = "johndoe",
            FullName = "John Doe",
            Email = "john.doe@example.com"
        };
        _context.Students.Add(student);

        // Додаємо тести
        var mathTest = new Test
        {
            Name = "Mathematics Test",
            Description = "Basic mathematics test",
            TeacherID = 1,
            Password = "mathtest123",
            CreatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        var historyTest = new Test
        {
            Name = "History Test",
            Description = "World history test",
            TeacherID = 1,
            Password = "historytest123",
            CreatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        _context.Tests.AddRange(mathTest, historyTest);

        // Додаємо питання для математичного тесту
        var questions = new List<Question>
        {
            new Question
            {
                TestId = 1,
                Description = "What is 2 + 2?",
                Answers = new List<string> { "3", "4", "5", "6" },
                CorrectAnswerIndex = 1,
                Score = 10
            },
            new Question
            {
                TestId = 1,
                Description = "What is 5 * 3?",
                Answers = new List<string> { "12", "15", "18", "20" },
                CorrectAnswerIndex = 1,
                Score = 15
            },
            new Question
            {
                TestId = 1,
                Description = "What is 10 / 2?",
                Answers = new List<string> { "3", "4", "5", "6" },
                CorrectAnswerIndex = 2,
                Score = 5
            }
        };

        _context.Questions.AddRange(questions);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}