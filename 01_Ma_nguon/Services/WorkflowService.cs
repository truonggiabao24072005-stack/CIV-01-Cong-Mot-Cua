using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CongMotCua.Services;
public partial class WorkflowService(AppDb db, FileStore files, SlaService sla)
{
    public async Task<Actor> Actor(ClaimsPrincipal claims)
    {
        var sub = claims.FindFirstValue("sub") ?? throw new BusinessError("Cần đăng nhập.", 401);
        var issuer = claims.FindFirstValue("iss") ?? throw new BusinessError("Danh tính không có đơn vị xác thực.", 401);
        var roles = JsonSerializer.Deserialize<JsonElement>(claims.FindFirstValue("realm_access") ?? "{}");
        bool Has(string r) => roles.TryGetProperty("roles", out var list) && list.EnumerateArray().Any(v => v.GetString() == r);
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.Issuer == issuer && x.Subject == sub);
        if (user == null)
        {
            if (!Has("NGUOI_NOP"))
                throw new BusinessError("Cán bộ chưa được cấp hồ sơ tài khoản trong hệ thống.", 403);
            user = new()
            {
                Issuer = issuer,
                Subject = sub,
                HoTen = claims.FindFirstValue("name") ?? claims.FindFirstValue("preferred_username") ?? "Người dân",
                Email = claims.FindFirstValue("email"),
                VaiTro = "NGUOI_NOP"
            };
            db.Add(user);
            await db.SaveChangesAsync();
        }

        if (!user.HoatDong || !Has(user.VaiTro))
            throw new BusinessError("Tài khoản chưa có quyền nghiệp vụ hoặc đã ngừng hoạt động.", 403);
        return new(sub, user.HoTen, user.VaiTro, user.Id, user.BoPhanId, user.Email, user.SoDienThoai);
    }

    public IQueryable<HoSo> Visible(Actor actor)
    {
        if (actor.Citizen)
            return db.HoSo.Where(h => h.ChuHoSoId == actor.Id);
        if (actor.Reception)
            return db.HoSo.Where(h => h.TrangThai != "BAN_NHAP");
        // Khi bàn giao, cán bộ cũ mất quyền xem. Cán bộ xử lý cuối vẫn đọc được
        // kết quả của mình sau khi phân công đã đóng, nhưng không sửa hồ sơ.
        return db.HoSo.Where(h => db.PhanCongXuLy.Any(p => p.HoSoId == h.Id && p.CanBoNhanId == actor.Id && p.KetThucLuc == null) || ((h.TrangThai == "DA_CO_KET_QUA" || h.TrangThai == "DA_TRA_KET_QUA" || h.TrangThai == "TU_CHOI_GIAI_QUYET") && db.PhanCongXuLy.Where(p => p.HoSoId == h.Id).OrderByDescending(p => p.BatDauLuc).ThenByDescending(p => p.Id).Select(p => p.CanBoNhanId).FirstOrDefault() == actor.Id));
    }

    public async Task<HoSo> Read(string id, Actor a) => await Visible(a).AsNoTracking().SingleOrDefaultAsync(h => h.Id == id) ?? throw new BusinessError("Không tìm thấy hồ sơ trong phạm vi quyền xem.", 404);
    public static string Required(string? value, string label, int max = 6000)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessError($"Cần nhập {label}.");
        if (value.Trim().Length > max)
            throw new BusinessError($"{label} không quá {max} ký tự.");
        return value.Trim();
    }

    public static object Summary(HoSo h) => new
    {
        h.Id,
        ma = h.MaHoSo ?? h.MaGui ?? "Bản nháp",
        h.MaGui,
        h.MaHoSo,
        h.TrangThai,
        h.NgayNop,
        h.NgayTiepNhan,
        h.HanTra,
        h.HanTraGoc,
        h.NgayKetThuc,
        h.ThongTinNguoiNop,
        version = h.RowVersion,
        noiDung = h.NoiDungDeNghi
    };
    void History(HoSo h, Actor a, string action, string before, string text, bool isPublic = true)
    {
        db.LichSuXuLy.Add(new() { HoSoId = h.Id, NguoiThucHienId = a.Id, HanhDong = action, TrangThaiTruoc = before, TrangThaiSau = h.TrangThai, NoiDung = text, CongKhai = isPublic, ThoiDiem = DateTime.UtcNow });
    }

    async Task<bool> Assigned(string id, Actor a) => !a.Citizen && await db.PhanCongXuLy.AnyAsync(x => x.HoSoId == id && x.CanBoNhanId == a.Id && x.KetThucLuc == null);
    async Task Handler(HoSo h, Actor a, string stage)
    {
        if (stage == "DA_GUI" && a.Reception)
            return;
        if (stage == "DANG_GIAI_QUYET" && a.VaiTro == "GIAI_QUYET" && await Assigned(h.Id, a))
            return;
        throw new BusinessError("Bạn không phụ trách thao tác ở giai đoạn này.", 403);
    }

    async Task LockVersion(HoSo h, Actor a, int version)
    {
        if (!await Visible(a).AnyAsync(x => x.Id == h.Id))
            throw new BusinessError("Không có quyền thao tác hồ sơ.", 403);
        if (h.RowVersion != version)
            throw new BusinessError("Hồ sơ vừa thay đổi. Vui lòng tải lại trước khi thao tác.", 409);
    }

    async Task<List<FileInput>> ValidateFiles(List<FileInput>? input, string procedure, Actor a)
    {
        var rows = input ?? [];
        if (rows.Count > 30)
            throw new BusinessError("Tối đa 30 tệp trong một lần gửi.");
        if (rows.Select(f => f.TepKey).Distinct().Count() != rows.Count || rows.Select(f => f.ThanhPhanId).Distinct().Count() != rows.Count)
            throw new BusinessError("Mỗi thành phần chỉ chọn một bản tệp trong một lần gửi.");
        var components = await db.ThanhPhanYeuCau.Where(x => x.ThuTucId == procedure).Select(x => x.Id).ToListAsync();
        foreach (var f in rows)
        {
            if (!components.Contains(f.ThanhPhanId))
                throw new BusinessError("Tài liệu không thuộc thành phần của thủ tục.");
            files.Owned(f.TepKey, a.Sub);
            if (await db.TaiLieuHoSo.AnyAsync(x => x.TepKey == f.TepKey) || await db.KetQuaHoSo.AnyAsync(x => x.TepKey == f.TepKey))
                throw new BusinessError("Bản tệp đã được sử dụng. Hãy tải lên bản mới.");
        }

        return rows;
    }

    async Task AddFiles(HoSo h, Actor a, List<FileInput> rows, string? lanId, bool current)
    {
        foreach (var f in rows)
        {
            var meta = files.Owned(f.TepKey, a.Sub);
            int version = (await db.TaiLieuHoSo.Where(x => x.HoSoId == h.Id && x.ThanhPhanId == f.ThanhPhanId).MaxAsync(x => (int? )x.SoPhienBan) ?? 0) + 1;
            if (current)
            {
                var old = await db.TaiLieuHoSo.Where(x => x.HoSoId == h.Id && x.ThanhPhanId == f.ThanhPhanId && x.HienHanh).ToListAsync();
                old.ForEach(x => x.HienHanh = false);
                await db.SaveChangesAsync();
            }

            db.Add(new TaiLieuHoSo { HoSoId = h.Id, ThanhPhanId = f.ThanhPhanId, LanId = lanId, SoPhienBan = version, TenTep = meta.Name, LoaiTep = meta.ContentType, DungLuong = checked((int)meta.Size), TepKey = meta.Key, HienHanh = current, TaoLuc = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }

    async Task CheckComponents(HoSo h, bool reviewed)
    {
        var required = await db.ThanhPhanYeuCau.Where(x => x.ThuTucId == h.ThuTucId && x.BatBuoc).ToListAsync();
        var docs = await db.TaiLieuHoSo.Where(x => x.HoSoId == h.Id && x.HienHanh).ToListAsync();
        if (required.Any(c => !docs.Any(d => d.ThanhPhanId == c.Id)))
            throw new BusinessError("Còn thiếu bản tài liệu hiện hành cho thành phần bắt buộc.");
        foreach (var d in docs)
        {
            files.Get(d.TepKey);
            if (reviewed)
            {
                var last = await db.KiemTraThanhPhan.Where(k => k.TaiLieuId == d.Id).OrderByDescending(k => k.ThoiDiem).FirstOrDefaultAsync();
                if (last?.KetQua != "DAT")
                    throw new BusinessError($"Cần đánh giá Đạt bản tài liệu {d.TenTep} trước khi tiếp nhận.");
            }
        }
    }

    public async Task<object> SaveDraft(string? id, DraftInput input, Actor a, int? version = null)
    {
        if (!a.Citizen)
            throw new BusinessError("Chỉ người dân được tạo hoặc sửa bản nháp.", 403);
        await using var tx = await db.Database.BeginTransactionAsync();
        var proc = await db.ThuTuc.FromSqlInterpolated($"SELECT * FROM [ThuTuc] WITH (HOLDLOCK,ROWLOCK) WHERE ThuTucId={input.ThuTucId}")
            .SingleOrDefaultAsync(t => t.HoatDong) ?? throw new BusinessError("Thủ tục không tồn tại hoặc đã ngừng tiếp nhận.");
        HoSo h;
        if (id == null)
        {
            h = new()
            {
                ThuTucId = proc.Id,
                ChuHoSoId = a.Id,
                TrangThai = "BAN_NHAP",
                ThongTinNguoiNop = JsonSerializer.Serialize(new { hoTen = a.Ten })
            };
            db.Add(h);
            await db.SaveChangesAsync();
        }
        else
        {
            h = await db.HoSo.FromSqlInterpolated($"SELECT * FROM [HoSo] WITH (UPDLOCK,ROWLOCK) WHERE HoSoId={id}").SingleOrDefaultAsync() ?? throw new BusinessError("Bản nháp không tồn tại.", 404);
            await LockVersion(h, a, version ?? -1);
            if (h.ChuHoSoId != a.Id || h.TrangThai != "BAN_NHAP")
                throw new BusinessError("Chỉ sửa được bản nháp của bạn.", 409);
            if (h.ThuTucId != proc.Id)
                throw new BusinessError("Tạo bản nháp mới nếu cần đổi thủ tục.");
        }

        if ((input.NoiDung?.Length ?? 0) > 6000)
            throw new BusinessError("Nội dung tối đa 6000 ký tự.");
        h.NoiDungDeNghi = input.NoiDung?.Trim();
        if (input.ThongTinLienHe != null)
        {
            if (input.ThongTinLienHe.SoDienThoai?.Length > 30 || input.ThongTinLienHe.DiaChi?.Length > 500)
                throw new BusinessError("Thông tin liên hệ vượt quá độ dài cho phép.");
            h.ThongTinNguoiNop = JsonSerializer.Serialize(new { hoTen = a.Ten, email = a.Email, soDienThoai = input.ThongTinLienHe.SoDienThoai?.Trim(), diaChi = input.ThongTinLienHe.DiaChi?.Trim() });
        }

        var rows = await ValidateFiles(input.TaiLieu, proc.Id, a);
        await AddFiles(h, a, rows, null, true);
        var before = h.TrangThai;
        if (input.Nop)
        {
            Required(h.NoiDungDeNghi, "nội dung đề nghị");
            await CheckComponents(h, false);
            h.MaGui = Code("GUI");
            h.NgayNop = DateTime.UtcNow;
            h.TrangThai = "DA_GUI";
        }

        if (id != null)
            h.RowVersion++;
        History(h, a, input.Nop ? "NOP_HO_SO" : "LUU_NHAP", before, input.Nop ? "Người dân nộp hồ sơ, chờ kiểm tra." : "Lưu bản nháp chưa nộp.");
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Summary(h);
    }

    public static string Code(string prefix) => $"{prefix}-{DateTime.UtcNow.AddHours(7):yyyyMMdd}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
    async Task CloseAssignment(string id)
    {
        var rows = await db.PhanCongXuLy.Where(x => x.HoSoId == id && x.KetThucLuc == null).ToListAsync();
        rows.ForEach(x => x.KetThucLuc = DateTime.UtcNow);
    }
}
