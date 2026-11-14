using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Controllers;

[ApiController, EnableRateLimiting("public"), Route("api/auth")]
public class XacThucController(LocalIdentity identity, AuthSettings settings, AppDb db) : ControllerBase
{
    [HttpPost("login")]
    public async Task<object> DangNhap(LoginInput input)
    {
        var account = await identity.Login(input);
        var user = await db.NguoiDung.AsNoTracking().SingleOrDefaultAsync(x =>
            x.Issuer == settings.Issuer && x.Subject == account.Subject);
        if (user is { HoatDong: false })
            throw new BusinessError("Tài khoản đã ngừng hoạt động.", 403);

        return identity.Token(account, settings.Issuer);
    }

    [HttpPost("register")]
    public async Task<object> DangKy(RegisterInput input)
    {
        return identity.Token(await identity.Register(input), settings.Issuer);
    }
}
