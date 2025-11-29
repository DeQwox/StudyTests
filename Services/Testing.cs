using System.Data;
<<<<<<< HEAD
=======
using System.Text.Json.Serialization;
>>>>>>> origin/lab6Artem
using System.Threading.Tasks;
using Repositories;
using StudyTests.Models.Entities;

<<<<<<< HEAD
namespace Services;

public class Testing
{
    private readonly ITestingRepository _testingRepository;
    private Test _test;
    private int[] _order = [];
    private int _current = 0;
    private List<Question> _questions = [];
    private int[] _answers = [];

    private int _studentId;
    private int _testId;

    public Testing(int testId, int studentId, ITestingRepository testingRepository)
    {
        _testId = testId;
        _studentId = studentId;
=======
namespace StudyTests.Services;

public class Testing
{
    [JsonIgnore]
    private ITestingRepository? _testingRepository;

    [JsonIgnore]
    private Test? _test;

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
>>>>>>> origin/lab6Artem

        _testingRepository = testingRepository;
        _test = _testingRepository.GetTestById(testId) ?? throw new Exception("Test doesn't exist");

<<<<<<< HEAD
        _questions = _testingRepository.GetTestQuestionList(testId).ToList();
        _order = Enumerable.Range(0, _questions.Count).ToArray();
        new Random().Shuffle(_order);
        
        
        _answers = new int[_questions.Count];
=======
        Questions = _testingRepository.GetTestQuestionList(testId).ToList();
        Order = Enumerable.Range(0, Questions.Count).ToArray();
        new Random().Shuffle(Order);
        
        
        Answers = new int[Questions.Count];
>>>>>>> origin/lab6Artem
    }

    public int GetQuestionsCount()
    {
<<<<<<< HEAD
        return _questions.Count;
=======
        return Questions.Count;
>>>>>>> origin/lab6Artem
    }

    public Question GetQuestion()
    {
<<<<<<< HEAD
        return _questions[_order[_current++]];
=======
        return Questions[Order[Current++]];
>>>>>>> origin/lab6Artem
    }

    public void Answer(int answer)
    {
<<<<<<< HEAD
        _answers[_order[_current - 1]] = answer;
=======
        Answers[Order[Current - 1]] = answer;
>>>>>>> origin/lab6Artem
    }
    
    public async Task<PassedTest> GetResult()
    {
        return new PassedTest()
        {
<<<<<<< HEAD
            Student = await _testingRepository.GetStudentByIdAsync(_studentId) ?? throw new Exception("Student doesn't exist"),
            Test    = await _testingRepository.GetTestByIdAsync(_testId) ?? throw new Exception("Test doesn't exist"),
            Answers = _answers.Select(i => i.ToString()).ToList(),
            Score = _questions.Zip(_answers)
=======
            Student = await _testingRepository!.GetStudentByIdAsync(StudentId) ?? throw new Exception("Student doesn't exist"),
            Test    = await _testingRepository.GetTestByIdAsync(TestId) ?? throw new Exception("Test doesn't exist"),
            Answers = Answers.Select(i => i.ToString()).ToList(),
            Score = Questions.Zip(Answers)
>>>>>>> origin/lab6Artem
                        .Select(i => (i.First.CorrectAnswerIndex == i.Second) ? i.First.Score : 0)
                        .Sum()
        };
    }
<<<<<<< HEAD
=======

    public void RestoreDependencies(ITestingRepository testingRepository)
    {
        _testingRepository = testingRepository;
        _test = _testingRepository.GetTestById(TestId) ?? throw new Exception("Test doesn't exist");
        Questions = _testingRepository.GetTestQuestionList(TestId).ToList();
    }
>>>>>>> origin/lab6Artem
}