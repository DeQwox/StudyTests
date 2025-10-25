using System.Text.Json;

namespace StudyTests.Data;

public static class JsonHelpers
{
    public static string SerializeAnswers(System.Collections.Generic.List<string> answers)
    {
        return JsonSerializer.Serialize(answers);
    }

    public static System.Collections.Generic.List<string> DeserializeAnswers(string json)
    {
        return JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json) ?? new System.Collections.Generic.List<string>();
    }
}
