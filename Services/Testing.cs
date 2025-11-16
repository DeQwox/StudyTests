using System.Data;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Repositories;
using StudyTests.Models.Entities;

namespace StudyTests.Services;

public class Testing
{
    [JsonIgnore]
    private ITestingRepository _testingRepository;

    [JsonIgnore]
    private Test _test;

    [JsonIgnore]
    public List<Question> Questions { get; set; } = [];
    
    public int[] Order { get; set; } = [];
    public int Current { get; set; }
    public int[] Answers { get; set; } = [];

    public int StudentId { get; set; }
    public int TestId { get; set; }

    public Testing() { }

    public Testing(int testId, int studentId, ITestingRepository testingRepository)
    {
        TestId = testId;
        Console.WriteLine($"Test id: {TestId}");
        StudentId = studentId;

        _testingRepository = testingRepository;
        _test = _testingRepository.GetTestById(testId) ?? throw new Exception("Test doesn't exist");

        Questions = _testingRepository.GetTestQuestionList(testId).ToList();
        Order = Enumerable.Range(0, Questions.Count).ToArray();
        new Random().Shuffle(Order);
        
        
        Answers = new int[Questions.Count];
    }

    public int GetQuestionsCount()
    {
        return Questions.Count;
    }

    public Question GetQuestion()
    {
        return Questions[Order[Current++]];
    }

    public void Answer(int answer)
    {
        Answers[Order[Current - 1]] = answer;
    }
    
    public async Task<PassedTest> GetResult()
    {
        return new PassedTest()
        {
            Student = await _testingRepository.GetStudentByIdAsync(StudentId) ?? throw new Exception("Student doesn't exist"),
            Test    = await _testingRepository.GetTestByIdAsync(TestId) ?? throw new Exception("Test doesn't exist"),
            Answers = Answers.Select(i => i.ToString()).ToList(),
            Score = Questions.Zip(Answers)
                        .Select(i => (i.First.CorrectAnswerIndex == i.Second) ? i.First.Score : 0)
                        .Sum()
        };
    }

    public void RestoreDependencies(ITestingRepository testingRepository)
    {
        _testingRepository = testingRepository;
        _test = _testingRepository.GetTestById(TestId) ?? throw new Exception("Test doesn't exist");
        Questions = _testingRepository.GetTestQuestionList(TestId).ToList();
    }
}