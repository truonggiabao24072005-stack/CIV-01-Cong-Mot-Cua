using System.Collections.Concurrent;
using System.Security.Cryptography;
using CongMotCua.Api.Filters;

namespace CongMotCua.Services;

public class ThuThachService
{
    private readonly ConcurrentDictionary<string, (string Answer, DateTime Until)> challenges = new();

    public object Tao()
    {
        var now = DateTime.UtcNow;
        foreach (var expired in challenges.Where(x => x.Value.Until < now).Take(100))
            challenges.TryRemove(expired.Key, out _);

        var x = RandomNumberGenerator.GetInt32(1, 20);
        var y = RandomNumberGenerator.GetInt32(1, 20);
        var id = Guid.NewGuid().ToString("N");
        challenges[id] = ((x + y).ToString(), now.AddMinutes(3));
        return new { id, cauHoi = $"{x} + {y} = ?" };
    }

    public void KiemTra(string id, string answer)
    {
        if (!challenges.TryRemove(id, out var check) || check.Until < DateTime.UtcNow || check.Answer != answer.Trim())
            throw new BusinessError("Câu trả lời chưa đúng hoặc thử thách đã hết hạn.");
    }
}
