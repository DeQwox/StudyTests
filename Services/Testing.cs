using System.Data;
using System.Threading.Tasks;
using Repositories;
using StudyTests.Models.Entities;

namespace StudyTests.Services;

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

        _testingRepository = testingRepository;
        _test = _testingRepository.GetTestById(testId) ?? throw new Exception("Test doesn't exist");

        _questions = _testingRepository.GetTestQuestionList(testId).ToList();
        _order = Enumerable.Range(0, _questions.Count).ToArray();
        new Random().Shuffle(_order);
        
        
        _answers = new int[_questions.Count];
    }

    public int GetQuestionsCount()
    {
        return _questions.Count;
    }

    public Question GetQuestion()
    {
        return _questions[_order[_current++]];
    }

    public void Answer(int answer)
    {
        _answers[_order[_current - 1]] = answer;
    }
    
    public async Task<PassedTest> GetResult()
    {
        return new PassedTest()
        {
            Student = await _testingRepository.GetStudentByIdAsync(_studentId) ?? throw new Exception("Student doesn't exist"),
            Test    = await _testingRepository.GetTestByIdAsync(_testId) ?? throw new Exception("Test doesn't exist"),
            Answers = _answers.Select(i => i.ToString()).ToList(),
            Score = _questions.Zip(_answers)
                        .Select(i => (i.First.CorrectAnswerIndex == i.Second) ? i.First.Score : 0)
                        .Sum()
        };
    }
}