using CongMotCua.Api.Models.Data;
using CongMotCua.Services;
using Microsoft.AspNetCore.Mvc;

namespace CongMotCua.Api.Controllers;

[ApiController, Route("api")]
public class HeThongController(AppDb db, IConfiguration configuration, AuthSettings auth) : ControllerBase
{
    [HttpGet("health")]
    public async Task<IResult> GetTrangThai()
    {
        var connected = await db.Database.CanConnectAsync();
        return Results.Ok(new { status = connected ? "ok" : "error", database = "SQL Server", version = "2.0.0", tables = 13 });
    }

    [HttpGet("config")]
    public object GetCauHinh()
    {
        return new
        {
            authMode = auth.IsLocal ? "local" : "keycloak",
            keycloakUrl = configuration["Auth:PublicUrl"] ?? "http://localhost:8180",
            realm = "congmotcua",
            clientId = "cong-mot-cua-web"
        };
    }
}
