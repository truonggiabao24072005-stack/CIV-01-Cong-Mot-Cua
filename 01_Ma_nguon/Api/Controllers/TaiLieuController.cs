using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using CongMotCua.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Api.Controllers;

[ApiController, Authorize, Route("api")]
public class TaiLieuController(WorkflowService workflow, AppDb db, FileStore files, ILogger<TaiLieuController> logger) : ControllerBase
{
    [HttpPost("tep")]
    public async Task<IResult> TaiLen()
    {
        var actor = await workflow.Actor(User);
        if (!Request.HasFormContentType)
            throw new BusinessError("Cần gửi multipart/form-data.");

        var form = await Request.ReadFormAsync();
        var file = form.Files.GetFile("file") ?? throw new BusinessError("Chưa chọn tệp.");
        return Results.Ok(await files.Put(file, actor.Sub));
    }

    [HttpGet("tai-lieu/{id}")]
    public async Task<IResult> TaiTaiLieu(string id)
    {
        var actor = await workflow.Actor(User);
        var taiLieu = await db.TaiLieuHoSo.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new BusinessError("Không tìm thấy tệp.", 404);
        await workflow.Read(taiLieu.HoSoId, actor);

        var info = files.Get(taiLieu.TepKey);
        return Results.File(files.PathFor(taiLieu.TepKey), info.ContentType, info.Name);
    }

    [HttpGet("ho-so/{id}/tep-ket-qua")]
    public async Task<IResult> TaiKetQua(string id)
    {
        var actor = await workflow.Actor(User);
        await workflow.Read(id, actor);
        var ketQua = await db.KetQuaHoSo.AsNoTracking().SingleOrDefaultAsync(x => x.HoSoId == id)
            ?? throw new BusinessError("Chưa có kết quả.", 404);
        if (actor.Citizen && ketQua.PhatHanhLuc == null)
            throw new BusinessError("Kết quả chưa được phát hành.", 403);
        if (ketQua.TepKey == null)
            throw new BusinessError("Không có tệp kết quả.", 404);

        var info = files.Get(ketQua.TepKey);
        logger.LogInformation("Result download {Case} by {Actor}", id, actor.Id);
        return Results.File(files.PathFor(ketQua.TepKey), info.ContentType, info.Name);
    }
}
