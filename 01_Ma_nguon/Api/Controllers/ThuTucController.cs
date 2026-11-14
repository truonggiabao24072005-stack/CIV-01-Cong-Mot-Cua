using CongMotCua.Api.Models.Data;
using CongMotCua.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Controllers;

[ApiController, Route("api/thu-tuc")]
public class ThuTucController(AppDb db) : ControllerBase
{
    [HttpGet]
    public async Task<object> GetThuTuc()
    {
        return await db.ThuTuc.AsNoTracking().Where(x => x.HoatDong).OrderBy(x => x.MaThuTuc)
            .Select(x => new { x.Id, ma = x.MaThuTuc, ten = x.TenThuTuc, x.MoTa, x.SoNgaySLA, x.DonViSLA, x.QuyTacBoSung, x.BoPhanChuyenMonId })
            .ToListAsync();
    }

    [HttpGet("{id}/thanh-phan")]
    public async Task<object> GetThanhPhan(string id)
    {
        if (!await db.ThuTuc.AnyAsync(x => x.Id == id && x.HoatDong))
            throw new BusinessError("Không tìm thấy thủ tục đang tiếp nhận.", 404);
        return await db.ThanhPhanYeuCau.AsNoTracking().Where(x => x.ThuTucId == id).OrderBy(x => x.ThuTu)
            .Select(x => new { x.Id, ten = x.TenThanhPhan, x.BatBuoc, x.HuongDan }).ToListAsync();
    }
}
