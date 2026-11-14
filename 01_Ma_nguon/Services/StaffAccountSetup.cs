using System.Text;
using CongMotCua.Api.Models.Data;

namespace CongMotCua.Services;

internal static class StaffAccountSetup
{
    public static async Task Run(AppDb db, LocalIdentity identity, AuthSettings auth, string[] args)
    {
        var fields = args.SkipWhile(arg => arg != "--cap-tai-khoan").Skip(1).ToArray();
        if (fields.Length is < 4 or > 5)
            throw new InvalidOperationException("Cần nhập tên đăng nhập, họ tên, email, vai trò và mã bộ phận nếu có.");

        var password = ReadPassword("Mật khẩu (12–128 ký tự): ");
        if (!Console.IsInputRedirected && password != ReadPassword("Nhập lại mật khẩu: "))
            throw new InvalidOperationException("Hai mật khẩu không giống nhau.");

        await identity.CreateStaff(db, auth.Issuer, fields[0], fields[1], fields[2], fields[3], fields.ElementAtOrDefault(4), password);
        Console.WriteLine("Đã cấp tài khoản cán bộ.");
    }

    private static string ReadPassword(string prompt)
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? "";

        Console.Write(prompt);
        var value = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Backspace && value.Length > 0)
                value.Length--;
            else if (!char.IsControl(key.KeyChar))
                value.Append(key.KeyChar);
        }
        Console.WriteLine();
        return value.ToString();
    }
}
