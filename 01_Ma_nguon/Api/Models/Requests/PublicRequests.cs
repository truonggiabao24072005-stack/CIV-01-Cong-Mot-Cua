namespace CongMotCua.Api.Models.Requests;
public record PublicLookup(string Code, string ChallengeId, string Answer);
public record ProfileInput(string? SoDienThoai);
