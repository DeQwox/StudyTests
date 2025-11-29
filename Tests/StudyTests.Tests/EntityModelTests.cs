using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using StudyTests.Models.Entities;
using Xunit;

namespace StudyTests.Tests;

public class EntityModelTests
{
    [Fact]
    public void Test_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var test = new Test
        {
            Id = 1,
            Name = "Sample Test",
            Description = "Sample Description",
            TeacherID = 1,
            Password = "password123",
            CreatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        test.Id.Should().Be(1);
        test.Name.Should().Be("Sample Test");
        test.Description.Should().Be("Sample Description");
        test.TeacherID.Should().Be(1);
        test.Password.Should().Be("password123");
        test.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        test.ValidUntil.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(1));
        test.Questions.Should().NotBeNull();
        test.Questions.Should().BeEmpty();
    }

    [Fact]
    public void Question_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var question = new Question
        {
            Id = 1,
            TestId = 1,
            Description = "What is 2 + 2?",
            Answers = new List<string> { "3", "4", "5", "6" },
            CorrectAnswerIndex = 1,
            Score = 10.5
        };

        // Assert
        question.Id.Should().Be(1);
        question.TestId.Should().Be(1);
        question.Description.Should().Be("What is 2 + 2?");
        question.Answers.Should().HaveCount(4);
        question.Answers.Should().Contain("4");
        question.CorrectAnswerIndex.Should().Be(1);
        question.Score.Should().Be(10.5);
    }

    [Fact]
    public void Student_ShouldInheritFromUser()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = 1,
            Login = "johndoe",
            FullName = "John Doe",
            Email = "john.doe@example.com"
        };

        // Assert
        student.Should().BeAssignableTo<User>();
        student.Id.Should().Be(1);
        student.Login.Should().Be("johndoe");
        student.FullName.Should().Be("John Doe");
        student.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Teacher_ShouldInheritFromUser()
    {
        // Arrange & Act
        var teacher = new Teacher
        {
            Id = 1,
            Login = "janesmith",
            FullName = "Jane Smith",
            Email = "jane.smith@example.com"
        };

        // Assert
        teacher.Should().BeAssignableTo<User>();
        teacher.Id.Should().Be(1);
        teacher.Login.Should().Be("janesmith");
        teacher.FullName.Should().Be("Jane Smith");
        teacher.Email.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public void PassedTest_ShouldHaveValidProperties()
    {
        // Arrange
        var student = new Student
        {
            Id = 1,
            Login = "testStudent",
            FullName = "Test Student",
            Email = "test@example.com"
        };

        var test = new Test
        {
            Id = 1,
            Name = "Sample Test",
            Description = "Sample Description",
            TeacherID = 1,
            Password = "password123"
        };

        // Act
        var passedTest = new PassedTest
        {
            Id = 1,
            Student = student,
            Test = test,
            Answers = new List<string> { "1", "2", "1", "3" },
            Score = 85.5
        };

        // Assert
        passedTest.Id.Should().Be(1);
        passedTest.Student.Should().Be(student);
        passedTest.Test.Should().Be(test);
        passedTest.Answers.Should().HaveCount(4);
        passedTest.Answers.Should().Contain("1");
        passedTest.Score.Should().Be(85.5);
    }

    [Fact]
    public void User_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            FullName = "Test User",
            Email = "test.user@example.com",
            Password = "Password123!!!"
        };

        // Assert
        // user.Should().BeAssignableTo<Entity>();
        user.Id.Should().Be(1);
        user.Login.Should().Be("testuser");
        user.FullName.Should().Be("Test User");
        user.Email.Should().Be("test.user@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    public void User_WithInvalidEmail_ShouldStillCreateObject(string invalidEmail)
    {
        // Arrange & Act
        var user = new User
        {
            Login = "testuser",
            FullName = "Test User",
            Email = invalidEmail
        };

        // Assert
        // Об'єкт створюється успішно, але валідація відбувається на рівні атрибутів
        user.Email.Should().Be(invalidEmail);
    }

    [Fact]
    public void Question_WithEmptyAnswers_ShouldHaveEmptyList()
    {
        // Arrange & Act
        var question = new Question
        {
            TestId = 1,
            Description = "Test question",
            CorrectAnswerIndex = 0,
            Score = 5
        };

        // Assert
        question.Answers.Should().NotBeNull();
        question.Answers.Should().BeEmpty();
    }

    [Fact]
    public void Test_WithQuestions_ShouldMaintainRelationship()
    {
        // Arrange
        var test = new Test
        {
            Id = 1,
            Name = "Math Test",
            Description = "Basic math",
            TeacherID = 1,
            Password = "pass123"
        };

        var question1 = new Question
        {
            Id = 1,
            TestId = 1,
            Description = "What is 1+1?",
            Answers = new List<string> { "1", "2", "3" },
            CorrectAnswerIndex = 1,
            Score = 10,
            Test = test
        };

        var question2 = new Question
        {
            Id = 2,
            TestId = 1,
            Description = "What is 2+2?",
            Answers = new List<string> { "3", "4", "5" },
            CorrectAnswerIndex = 1,
            Score = 10,
            Test = test
        };

        // Act
        test.Questions = new List<Question> { question1, question2 };

        // Assert
        test.Questions.Should().HaveCount(2);
        test.Questions.Should().Contain(question1);
        test.Questions.Should().Contain(question2);
        question1.Test.Should().Be(test);
        question2.Test.Should().Be(test);
    }
}