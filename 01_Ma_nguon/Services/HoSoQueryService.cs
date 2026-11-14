using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Services;
public sealed class HoSoQueryService(AppDb db, WorkflowService workflow)
{
    IQueryable<HoSo> SupplementWork(IQueryable<HoSo> query, Actor actor)
    {
        var pending = query.Where(h => h.TrangThai == "CHO_BO_SUNG");
        if (actor.Citizen)
            return pending.Where(h => db.YeuCauBoSung.Any(y => y.HoSoId == h.Id && y.DongLuc == null && y.TrangThai == "DANG_CHO"));

        var stage = actor.Reception ? "DA_GUI" : "DANG_GIAI_QUYET";
        return pending.Where(h => db.YeuCauBoSung.Any(y => y.HoSoId == h.Id && y.DongLuc == null && y.TrangThai == "DA_GUI" && y.TrangThaiQuayLai == stage));
    }

    public async Task<object> List(Actor actor, string? q, string? status, string? deadline, string? work, int page, int pageSize)
    {
        var a = actor;
        var query = workflow.Visible(a).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(h => (h.MaGui != null && h.MaGui.Contains(q)) || (h.MaHoSo != null && h.MaHoSo.Contains(q)) || (h.NoiDungDeNghi != null && h.NoiDungDeNghi.Contains(q)) || db.ThuTuc.Any(t => t.Id == h.ThuTucId && t.TenThuTuc.Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(h => h.TrangThai == status);
        if (work == "supplement")
            query = SupplementWork(query, a);
        if (deadline == "overdue")
            query = query.Where(h => h.HanTra < DateTime.UtcNow && h.NgayKetThuc == null);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var total = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling((double)total / pageSize)));
        var items = await (
            from h in query
            join t in db.ThuTuc on h.ThuTucId equals t.Id
            join n in db.NguoiDung on h.ChuHoSoId equals n.Id
            orderby h.NgayNop descending, h.Id
            select new
            {
                h.Id,
                ma = h.MaHoSo ?? h.MaGui ?? "Bản nháp",
                h.MaGui,
                h.MaHoSo,
                h.TrangThai,
                h.NgayNop,
                h.HanTra,
                boSung = db.YeuCauBoSung.Where(y => y.HoSoId == h.Id && y.DongLuc == null).Select(y => new
                {
                    y.TrangThai,
                    y.TrangThaiQuayLai
                }).FirstOrDefault(),
                version = h.RowVersion,
                thuTuc = t.TenThuTuc,
                nguoiNop = n.HoTen,
                canBo = (
                    from l in db.PhanCongXuLy
                    join cb in db.NguoiDung on l.CanBoNhanId equals cb.Id
                    where l.HoSoId == h.Id && l.KetThucLuc == null
                    select cb.HoTen).FirstOrDefault()
            }

        ).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (new
        {
            items,
            total,
            page,
            pageSize
        }

        );
    }

    public async Task<object> Statistics(Actor actor)
    {
        var a = actor;
        var q = workflow.Visible(a).AsNoTracking();
        return new
        {
            tong = await q.CountAsync(),
            dangXuLy = await q.CountAsync(h => h.NgayKetThuc == null && h.TrangThai != "BAN_NHAP" && h.TrangThai != "TU_CHOI_TIEP_NHAN"),
            choBoSung = await q.CountAsync(h => h.TrangThai == "CHO_BO_SUNG"),
            boSungCanThaoTac = await SupplementWork(q, a).CountAsync(),
            daHoanTat = await q.CountAsync(h => h.TrangThai == "DA_TRA_KET_QUA"),
            quaHan = await q.CountAsync(h => h.HanTra < DateTime.UtcNow && h.NgayKetThuc == null),
            theoTrangThai = await q.GroupBy(h => h.TrangThai).Select(g => new { trangThai = g.Key, soLuong = g.Count() }).ToListAsync()
        };
    }
}
