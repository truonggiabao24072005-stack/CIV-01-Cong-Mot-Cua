namespace CongMotCua.Api.Models.Requests;
public record LoginInput(string Username, string Password);
public record RegisterInput(string Username, string Password, string HoTen, string Email);
