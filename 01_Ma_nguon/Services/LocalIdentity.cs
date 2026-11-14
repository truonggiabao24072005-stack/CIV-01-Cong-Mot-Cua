using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Services;
// Tài khoản cục bộ được lưu ngoài thư mục công khai của web.
public record LocalAccount(string Username, string Subject, string Name, string Email, string Role, string Hash, string Salt);
public class LocalIdentity
{
    readonly string folder;
    readonly IConfiguration configuration;
    readonly SemaphoreSlim gate = new(1, 1);
    public byte[] Key { get; }

    public LocalIdentity(IConfiguration cfg, IHostEnvironment environment)
    {
        configuration = cfg;
        folder = Path.GetFullPath(cfg["Identity:Root"] ?? ".data/identity", environment.ContentRootPath);
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "signing.key");
        if (!File.Exists(path))
            File.WriteAllBytes(path, RandomNumberGenerator.GetBytes(64));
        Key = File.ReadAllBytes(path);
    }

    string PathForAccounts => Path.Combine(folder, "accounts.json");

    async Task<List<LocalAccount>> Load() => File.Exists(PathForAccounts) ? JsonSerializer.Deserialize<List<LocalAccount>>(await File.ReadAllTextAsync(PathForAccounts)) ?? [] : [];
    async Task Save(List<LocalAccount> rows)
    {
        var temp = PathForAccounts + ".tmp";
        await File.WriteAllTextAsync(temp, JsonSerializer.Serialize(rows));
        File.Move(temp, PathForAccounts, true);
    }

    static LocalAccount New(string user, string sub, string name, string email, string role, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(24);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA256, 32);
        return new(user, sub, name, email, role, Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public async Task Seed()
    {
        await gate.WaitAsync();
        try
        {
            var rows = await Load();
            foreach (var x in new[]
            {
                ("minhanh", "11111111-1111-4111-8111-111111111111", "Nguyễn Minh Anh", "NGUOI_NOP"),
                ("hoangnam", "22222222-2222-4222-8222-222222222222", "Trần Hoàng Nam", "NGUOI_NOP"),
                ("tiepnhan", "33333333-3333-4333-8333-333333333333", "Lê Thu Hà", "TIEP_NHAN"),
                ("giaiquyet", "44444444-4444-4444-8444-444444444444", "Phạm Đức Huy", "GIAI_QUYET"),
                ("ngoclinh", "55555555-5555-4555-8555-555555555555", "Võ Ngọc Linh", "GIAI_QUYET")
            }

            )
                if (!rows.Any(a => a.Username == x.Item1))
                    {
                    var demoPassword = configuration["Identity:SamplePassword"];
                    if (string.IsNullOrWhiteSpace(demoPassword) || demoPassword.Length < 12)
                        throw new InvalidOperationException("Thiếu mật khẩu dữ liệu mẫu. Dùng tùy chọn -NapDuLieuMau khi cài đặt cơ sở dữ liệu riêng.");
                    rows.Add(New(x.Item1, x.Item2, x.Item3, x.Item1 + "@example.test", x.Item4, demoPassword));
                }
            await Save(rows);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<LocalAccount> Login(LoginInput input)
    {
        var user = input.Username.Trim().ToLowerInvariant();
        var a = (await Load()).FirstOrDefault(x => x.Username == user);
        var salt = a == null ? new byte[24] : Convert.FromBase64String(a.Salt);
        var actual = Rfc2898DeriveBytes.Pbkdf2(input.Password, salt, 210000, HashAlgorithmName.SHA256, 32);
        if (a == null || !CryptographicOperations.FixedTimeEquals(actual, Convert.FromBase64String(a.Hash)))
            throw new BusinessError("Tên đăng nhập hoặc mật khẩu chưa đúng.", 401);
        return a;
    }

    public async Task<LocalAccount> Register(RegisterInput input)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(input.Username, "^[a-zA-Z0-9._-]{4,40}$") || input.Password.Length is < 12 or > 128 || string.IsNullOrWhiteSpace(input.HoTen) || input.HoTen.Length > 100 || !System.Net.Mail.MailAddress.TryCreate(input.Email, out _))
            throw new BusinessError("Tên đăng nhập 4–40 ký tự, mật khẩu 12–128 ký tự, họ tên và email hợp lệ là bắt buộc.");
        await gate.WaitAsync();
        try
        {
            var rows = await Load();
            var user = input.Username.ToLowerInvariant();
            if (rows.Any(x => x.Username == user))
                throw new BusinessError("Tên đăng nhập đã tồn tại.", 409);
            var a = New(user, Guid.NewGuid().ToString(), input.HoTen.Trim(), input.Email.Trim(), "NGUOI_NOP", input.Password);
            rows.Add(a);
            await Save(rows);
            return a;
        }
        finally
        {
            gate.Release();
        }
    }

    internal async Task CreateStaff(AppDb db, string issuer, string username, string name, string email, string role, string? departmentCode, string password)
    {
        if (role is not ("TIEP_NHAN" or "GIAI_QUYET"))
            throw new BusinessError("Chỉ cấp tài khoản tiếp nhận hoặc chuyên môn qua công cụ cài đặt.");
        username = username.Trim().ToLowerInvariant();
        name = name.Trim();
        email = email.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, "^[a-z0-9._-]{4,40}$") || password.Length is < 12 or > 128 || name.Length is < 1 or > 100 || email.Length > 255 || !System.Net.Mail.MailAddress.TryCreate(email, out _))
            throw new BusinessError("Cần tên đăng nhập 4–40 ký tự, mật khẩu 12–128 ký tự, họ tên và email hợp lệ.");

        string? departmentId = null;
        if (!string.IsNullOrWhiteSpace(departmentCode))
            departmentId = await db.BoPhan.Where(x => x.MaBoPhan == departmentCode.Trim() && x.HoatDong)
                .Select(x => x.Id).SingleOrDefaultAsync() ?? throw new BusinessError("Không tìm thấy bộ phận đang hoạt động với mã đã nhập.");
        else if (role == "GIAI_QUYET")
            throw new BusinessError("Cán bộ chuyên môn phải thuộc một bộ phận đang hoạt động.");

        // Công cụ được chạy khi web đã dừng để chỉ có một tiến trình ghi kho danh tính.
        var rows = await Load();
        if (rows.Any(row => row.Username == username))
            throw new BusinessError("Tên đăng nhập đã tồn tại.", 409);

        var account = New(username, Guid.NewGuid().ToString(), name, email, role, password);
        var person = new NguoiDung { Issuer = issuer, Subject = account.Subject, HoTen = name, Email = email, VaiTro = role, BoPhanId = departmentId };
        db.NguoiDung.Add(person);
        await db.SaveChangesAsync();
        rows.Add(account);
        try
        {
            await Save(rows);
        }
        catch
        {
            db.NguoiDung.Remove(person);
            await db.SaveChangesAsync();
            throw;
        }
    }

    public object Token(LocalAccount a, string issuer)
    {
        var token = new JwtSecurityToken(issuer, "cong-mot-cua-api", new[] { new Claim("sub", a.Subject), new Claim("name", a.Name), new Claim("email", a.Email), new Claim("preferred_username", a.Username), new Claim("realm_access", JsonSerializer.Serialize(new { roles = new[] { a.Role } })) }, expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Key), SecurityAlgorithms.HmacSha256));
        return new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(token),
            expiresIn = 3600
        };
    }
}
