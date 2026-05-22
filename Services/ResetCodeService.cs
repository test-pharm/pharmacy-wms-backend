using System.Collections.Concurrent;

namespace PharmacyWmsBackend.Services;

public class ResetCodeService
{
    private static readonly ConcurrentDictionary<string, (string Code, DateTime Expires)> _codes = new();

    public string GenerateCode(string email)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        _codes[email.ToLower()] = (code, DateTime.UtcNow.AddMinutes(15));
        return code;
    }

    public bool VerifyCode(string email, string code)
    {
        var key = email.ToLower();
        if (!_codes.TryGetValue(key, out var entry)) return false;
        if (entry.Code != code || DateTime.UtcNow > entry.Expires)
        {
            _codes.TryRemove(key, out _);
            return false;
        }
        _codes.TryRemove(key, out _);
        return true;
    }
}
