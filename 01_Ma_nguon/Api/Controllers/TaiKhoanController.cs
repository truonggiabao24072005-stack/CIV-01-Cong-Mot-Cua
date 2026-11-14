using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class TaiKhoanController(WorkflowService workflow, AppDb db) : ControllerBase
{
    [HttpGet("me")]
    public Task<Actor> GetTaiKhoan()
    {
        return workflow.Actor(User);
    }

    [HttpPut("me")]
    public async Task<IResult> SuaThongTin(ProfileInput input)
    {
        var actor = await workflow.Actor(User);
        var user = await db.NguoiDung.FindAsync(actor.Id);
        user!.SoDienThoai = input.SoDienThoai?.Trim();
        if (user.SoDienThoai?.Length > 30)
            throw new BusinessError("Số điện thoại quá dài.");

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Đã lưu thông tin liên hệ." });
    }

    [HttpGet("can-bo")]
    public async Task<object> GetCanBo()
    {
        var actor = await workflow.Actor(User);
        if (actor.Citizen)
            throw new BusinessError("Không có quyền xem cán bộ.", 403);

        return await (
            from user in db.NguoiDung.AsNoTracking()
            join boPhan in db.BoPhan on user.BoPhanId equals boPhan.Id
            where user.VaiTro == "GIAI_QUYET" && user.HoatDong && boPhan.HoatDong
            select new { user.Id, user.HoTen, user.BoPhanId, boPhan = boPhan.TenBoPhan }
        ).ToListAsync();
    }
}
