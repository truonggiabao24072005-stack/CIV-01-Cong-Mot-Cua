using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Controllers;

[ApiController, EnableRateLimiting("public"), Route("api/tra-cuu")]
public class TraCuuController(AppDb db, ThuThachService thuThach) : ControllerBase
{
    [HttpGet("thu-thach")]
    public object GetThuThach()
    {
        return thuThach.Tao();
    }

    [HttpPost]
    public async Task<object> TraCuu(PublicLookup input)
    {
        thuThach.KiemTra(input.ChallengeId, input.Answer);
        var hoSo = await db.HoSo.AsNoTracking().FirstOrDefaultAsync(x =>
            (x.MaGui == input.Code || x.MaHoSo == input.Code) && x.TrangThai != "BAN_NHAP")
            ?? throw new BusinessError("Không tìm thấy mã hồ sơ.", 404);

        return new
        {
            ma = hoSo.MaHoSo ?? hoSo.MaGui,
            hoSo.TrangThai,
            hoSo.NgayNop,
            hoSo.NgayTiepNhan,
            hoSo.HanTra,
            hoSo.NgayKetThuc
        };
    }
}
