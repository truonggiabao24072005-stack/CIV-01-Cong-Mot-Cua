using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Services;
public sealed class CatalogService(AppDb db)
{
    private static void EnsureReception(Actor actor)
    {
        if (!actor.Reception)
            throw new BusinessError("Chỉ cán bộ tiếp nhận được quản lý danh mục.", 403);
    }

    public async Task<object> GetCatalog(Actor actor)
    {
        EnsureReception(actor);
        return new
        {
            boPhan = await db.BoPhan.AsNoTracking().OrderBy(x => x.MaBoPhan).ToListAsync(),
            thuTuc = await db.ThuTuc.AsNoTracking().OrderBy(x => x.MaThuTuc).ToListAsync(),
            thanhPhan = await db.ThanhPhanYeuCau.AsNoTracking().OrderBy(x => x.ThuTu).ToListAsync(),
            nguoiDung = await db.NguoiDung.AsNoTracking().Select(x => new { x.Id, x.HoTen, x.Email, x.SoDienThoai, x.VaiTro, x.BoPhanId, x.HoatDong }).ToListAsync()
        };
    }

    public async Task<object> SaveDepartment(DepartmentInput input, Actor actor)
    {
        EnsureReception(actor);
        await using var tx = await db.Database.BeginTransactionAsync();
        BoPhan row;
        if (input.Id == null)
        {
            row = new();
            db.Add(row);
        }
        else
            row = await LockDepartment(input.Id) ?? throw new BusinessError("Không tìm thấy bộ phận.", 404);
        if (!input.HoatDong && (await db.ThuTuc.AnyAsync(x => x.BoPhanChuyenMonId == row.Id && x.HoatDong) ||
            await db.HoSo.AnyAsync(h => (h.TrangThai == "DA_GUI" || h.TrangThai == "DA_TIEP_NHAN" || h.TrangThai == "CHO_BO_SUNG" || h.TrangThai == "DANG_GIAI_QUYET") &&
                db.ThuTuc.Any(t => t.Id == h.ThuTucId && t.BoPhanChuyenMonId == row.Id)) ||
            await db.PhanCongXuLy.AnyAsync(x => x.BoPhanNhanId == row.Id && x.KetThucLuc == null)))
            throw new BusinessError("Bộ phận còn thủ tục đang sử dụng, hồ sơ chờ xử lý hoặc phân công hiệu lực.");
        row.MaBoPhan = WorkflowService.Required(input.Ma, "mã bộ phận", 60);
        row.TenBoPhan = WorkflowService.Required(input.Ten, "tên bộ phận", 255);
        row.HoatDong = input.HoatDong;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (row);
    }

    Task<BoPhan?> LockDepartment(string id) => db.BoPhan
        .FromSqlInterpolated($"SELECT * FROM [BoPhan] WITH (UPDLOCK,ROWLOCK) WHERE BoPhanId={id}").SingleOrDefaultAsync();

    public async Task<object> SaveProcedure(ProcedureInput input, Actor actor)
    {
        EnsureReception(actor);
        if (input.SoNgaySLA is < 1 or > 365 || input.SoGioPhanHoi is < 1 or > 720 || input.DonViSLA is not ("NGAY_LICH" or "NGAY_LAM_VIEC") || input.QuyTacBoSung is not ("TINH_LAI" or "GIU_NGUYEN" or "TAM_DUNG"))
            throw new BusinessError("Cấu hình SLA không hợp lệ.");
        if (input.ThanhPhan.Count is < 1 or > 30)
            throw new BusinessError("Thủ tục cần từ 1 đến 30 thành phần.");
        var components = input.ThanhPhan.Select(x => new ComponentInput(
            WorkflowService.Required(x.Ten, "tên thành phần", 255), x.BatBuoc,
            WorkflowService.Required(x.HuongDan, "hướng dẫn"))).ToList();
        await using var tx = await db.Database.BeginTransactionAsync();
        var department = await LockDepartment(input.BoPhanId);
        ThuTuc row;
        if (input.Id == null)
        {
            row = new();
            db.Add(row);
        }
        else
            row = await db.ThuTuc.FromSqlInterpolated($"SELECT * FROM [ThuTuc] WITH (UPDLOCK,ROWLOCK) WHERE ThuTucId={input.Id}")
                .SingleOrDefaultAsync() ?? throw new BusinessError("Thủ tục không tồn tại.", 404);
        if (department == null || (!department.HoatDong && (input.HoatDong || row.BoPhanChuyenMonId != department.Id)))
            throw new BusinessError("Chọn bộ phận đang hoạt động.");
        var existing = await db.ThanhPhanYeuCau.Where(x => x.ThuTucId == row.Id).OrderBy(x => x.ThuTu).ToListAsync();
        var used = await db.HoSo.AnyAsync(x => x.ThuTucId == row.Id);
        var sameComponents = existing.Count == components.Count &&
            !existing.Where((x, i) => x.TenThanhPhan != components[i].Ten || x.BatBuoc != components[i].BatBuoc).Any();
        if (used && (row.BoPhanChuyenMonId != input.BoPhanId || !sameComponents))
            throw new BusinessError("Thủ tục đã có hồ sơ. Giữ bộ phận và danh sách thành phần; tạo mã thủ tục mới nếu thay đổi cấu trúc.");
        row.MaThuTuc = WorkflowService.Required(input.Ma, "mã thủ tục", 60);
        row.TenThuTuc = WorkflowService.Required(input.Ten, "tên thủ tục", 255);
        row.MoTa = WorkflowService.Required(input.MoTa, "mô tả");
        row.SoNgaySLA = input.SoNgaySLA;
        row.DonViSLA = input.DonViSLA;
        row.QuyTacBoSung = input.QuyTacBoSung;
        row.SoGioPhanHoi = input.SoGioPhanHoi;
        row.BoPhanChuyenMonId = input.BoPhanId;
        row.HoatDong = input.HoatDong;
        if (!sameComponents)
        {
            db.RemoveRange(existing);
            for (int i = 0; i < components.Count; i++)
            {
                var x = components[i];
                db.Add(new ThanhPhanYeuCau { ThuTucId = row.Id, TenThanhPhan = x.Ten, HuongDan = x.HuongDan, BatBuoc = x.BatBuoc, ThuTu = i + 1 });
            }
        }
        else
        {
            // Hướng dẫn có thể chỉnh sửa; không thay định danh hoặc cấu trúc
            // thành phần đang được tài liệu của hồ sơ tham chiếu.
            for (var i = 0; i < existing.Count; i++)
                existing[i].HuongDan = components[i].HuongDan;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (row);
    }

    public async Task<object> SetUserActive(UserAdminInput input, Actor actor)
    {
        EnsureReception(actor);
        var a = actor;
        var u = await db.NguoiDung.FindAsync(input.Id) ?? throw new BusinessError("Không tìm thấy người dùng.", 404);
        if (a.Id == u.Id && !input.HoatDong)
            throw new BusinessError("Không tự khóa tài khoản đang dùng.");
        if (!input.HoatDong && await db.PhanCongXuLy.AnyAsync(x => x.CanBoNhanId == u.Id && x.KetThucLuc == null))
            throw new BusinessError("Cán bộ còn phân công hiệu lực, cần bàn giao trước khi khóa.");
        u.HoatDong = input.HoatDong;
        await db.SaveChangesAsync();
        return (new
        {
            u.Id,
            u.HoatDong
        }

        );
    }

    public async Task<object> Report(Actor actor)
    {
        EnsureReception(actor);
        var now = DateTime.UtcNow;
        return await (
            from h in db.HoSo.AsNoTracking()join t in db.ThuTuc on h.ThuTucId equals t.Id
            where h.TrangThai != "BAN_NHAP"
            group h by new
            {
                t.Id,
                t.TenThuTuc
            }

                into g
                select new
                {
                    g.Key.Id,
                    thuTuc = g.Key.TenThuTuc,
                    tong = g.Count(),
                    daTra = g.Count(h => h.TrangThai == "DA_TRA_KET_QUA"),
                    tuChoi = g.Count(h => h.TrangThai == "TU_CHOI_TIEP_NHAN" || h.TrangThai == "TU_CHOI_GIAI_QUYET"),
                    quaHan = g.Count(h => h.HanTra < now && h.NgayKetThuc == null),
                    hoanTatDungHan = g.Count(h => h.NgayKetThuc != null && h.NgayKetThuc <= h.HanTra),
                    hoanTatTreHan = g.Count(h => h.NgayKetThuc > h.HanTra)
                }

        ).ToListAsync();
    }
}
