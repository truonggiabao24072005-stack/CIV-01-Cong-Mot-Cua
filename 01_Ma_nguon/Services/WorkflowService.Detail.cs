using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Services;
public partial class WorkflowService
{
    public async Task<object> Detail(string id, Actor a)
    {
        var h = await Read(id, a);
        var person = await db.NguoiDung.AsNoTracking().SingleAsync(x => x.Id == h.ChuHoSoId);
        var proc = await db.ThuTuc.AsNoTracking().SingleAsync(x => x.Id == h.ThuTucId);
        var people = await db.NguoiDung.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.HoTen);
        var components = await db.ThanhPhanYeuCau.AsNoTracking().Where(x => x.ThuTucId == h.ThuTucId).OrderBy(x => x.ThuTu).ToListAsync();
        var requests = await db.YeuCauBoSung.AsNoTracking().Where(x => x.HoSoId == id).OrderByDescending(x => x.TaoLuc).ToListAsync();
        var requestIds = requests.Select(x => x.Id).ToList();
        var details = await db.NoiDungBoSung.AsNoTracking().Where(x => requestIds.Contains(x.YeuCauId)).ToListAsync();
        var submissions = await db.LanBoSung.AsNoTracking().Where(x => requestIds.Contains(x.YeuCauId)).OrderByDescending(x => x.GuiLuc).ToListAsync();
        var docs = await db.TaiLieuHoSo.AsNoTracking().Where(x => x.HoSoId == id).OrderByDescending(x => x.TaoLuc).ToListAsync();
        var docIds = docs.Select(x => x.Id).ToList();
        var checks = await db.KiemTraThanhPhan.AsNoTracking().Where(x => docIds.Contains(x.TaiLieuId)).OrderByDescending(x => x.ThoiDiem).ToListAsync();
        var transfers = await db.PhanCongXuLy.AsNoTracking().Where(x => x.HoSoId == id).OrderByDescending(x => x.BatDauLuc).ToListAsync();
        var departments = await db.BoPhan.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.TenBoPhan);
        var histories = await db.LichSuXuLy.AsNoTracking().Where(x => x.HoSoId == id && (!a.Citizen || x.CongKhai)).OrderByDescending(x => x.ThoiDiem).ToListAsync();
        var result = await db.KetQuaHoSo.AsNoTracking().SingleOrDefaultAsync(x => x.HoSoId == id);
        if (a.Citizen && result?.PhatHanhLuc == null)
            result = null;
        return new
        {
            hoSo = Summary(h),
            nguoiNop = new
            {
                ten = person.HoTen,
                lienHe = person.Email,
                soDienThoai = person.SoDienThoai
            },
            thuTuc = new
            {
                id = proc.Id,
                ma = proc.MaThuTuc,
                ten = proc.TenThuTuc,
                proc.MoTa,
                proc.BoPhanChuyenMonId,
                proc.SoNgaySLA,
                proc.DonViSLA
            },
            thanhPhan = components.Select(x => new { x.Id, ten = x.TenThanhPhan, x.BatBuoc, x.HuongDan }),
            yeuCauBoSung = requests.Select(x => new { x.Id, x.TrangThai, x.TrangThaiQuayLai, x.TaoLuc, x.DongLuc, nguoiHuongDan = people.GetValueOrDefault(x.NguoiYeuCauId), noiDung = string.Join("; ", details.Where(d => d.YeuCauId == x.Id).Select(d => d.VanDe)), lyDo = string.Join("; ", details.Where(d => d.YeuCauId == x.Id).Select(d => d.LyDo)), huongDan = string.Join("; ", details.Where(d => d.YeuCauId == x.Id).Select(d => d.HuongDan)), noiDungChiTiet = details.Where(d => d.YeuCauId == x.Id), lanGui = submissions.Where(l => l.YeuCauId == x.Id), soLanGui = submissions.Count(l => l.YeuCauId == x.Id) }),
            taiLieu = docs.Select(d => new { d.Id, d.ThanhPhanId, thanhPhan = components.FirstOrDefault(c => c.Id == d.ThanhPhanId)?.TenThanhPhan, d.LanId, d.SoPhienBan, d.TenTep, d.LoaiTep, d.DungLuong, d.HienHanh, guiLuc = d.TaoLuc, kiemTra = checks.Where(k => k.TaiLieuId == d.Id).Select(k => new { k.Id, k.KetQua, k.NhanXet, k.ThoiDiem, canBo = people.GetValueOrDefault(k.CanBoId) }), choDanhGia = d.LanId != null && submissions.Any(l => l.Id == d.LanId && l.DanhGia == null) }),
            luanChuyen = transfers.Select(x => new { x.Id, x.CanBoNhanId, x.BoPhanNhanId, canBo = people.GetValueOrDefault(x.CanBoNhanId), boPhan = departments.GetValueOrDefault(x.BoPhanNhanId), chuyenLuc = x.BatDauLuc, x.KetThucLuc, noiDung = x.NoiDungChuyen }),
            lichSu = histories.Select(x => new { x.Id, x.HanhDong, x.TrangThaiTruoc, x.TrangThaiSau, x.NoiDung, x.ThoiDiem, nguoiThucHien = people.GetValueOrDefault(x.NguoiThucHienId) }),
            ketQua = result == null ? null : new
            {
                result.Id,
                loai = result.LoaiKetQua,
                noiDung = result.NoiDungKetLuan,
                result.LyDoTuChoi,
                result.TaoLuc,
                traLuc = result.PhatHanhLuc,
                nguoiPhatHanh = result.NguoiPhatHanhId == null ? null : people.GetValueOrDefault(result.NguoiPhatHanhId),
                coTep = result.TepKey != null
            },
            coTepKetQua = result?.TepKey != null,
            duocXuLy = await Assigned(id, a),
            sla = new
            {
                h.SoNgaySLA,
                h.DonViSLA,
                h.QuyTacBoSung,
                h.MocTinhSLA,
                h.LichApDung
            }
        };
    }
}
