using System.Collections.Concurrent;

namespace StudyTests.Services;

// Very small in-memory token store for demo purposes.
public static class SimpleTokenStore
{
    private record TokenEntry(int UserId, string Role, DateTime ExpiresAt);

    private static readonly ConcurrentDictionary<string, TokenEntry> _store = new();

    public static string Generate(int userId, string role, TimeSpan? lifetime = null)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
        var expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(8));
        _store[token] = new TokenEntry(userId, role, expires);
        return token;
    }

    public static bool TryValidate(string token, out int userId, out string role)
    {
        userId = 0; role = string.Empty;
        if (string.IsNullOrWhiteSpace(token)) return false;
        if (!_store.TryGetValue(token, out var entry)) return false;
        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _store.TryRemove(token, out _);
            return false;
        }
        userId = entry.UserId; role = entry.Role; return true;
    }
}