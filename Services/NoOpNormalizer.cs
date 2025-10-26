using Microsoft.AspNetCore.Identity;

namespace StudyTests.Services;

public class NoOpNormalizer : ILookupNormalizer
{
    public string NormalizeName(string? name) => name ?? "";
    public string NormalizeEmail(string? email) => email ?? "";
}