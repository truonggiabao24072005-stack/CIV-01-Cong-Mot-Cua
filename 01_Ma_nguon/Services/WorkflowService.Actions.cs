using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Api.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace CongMotCua.Services;
public partial class WorkflowService
{
    static string ResultNotice(ActionInput input, bool success)
    {
        var conclusion = Required(input.NoiDung, "kết luận");
        var basis = Required(input.CanCuXuLy, "căn cứ kiểm tra, đối chiếu", 1600);
        var guidance = Required(input.HuongDanThucHien, "hướng dẫn thực hiện", 1200);
        var sections = new List<string>
        {
            $"CĂN CỨ KIỂM TRA, ĐỐI CHIẾU\n{basis}",
            $"KẾT LUẬN GIẢI QUYẾT\n{conclusion}"
        };
        if (success)
            sections.Add($"KẾT QUẢ CỤ THỂ\n{Required(input.KetQuaCuThe, "kết quả cụ thể", 1600)}");
        sections.Add($"HƯỚNG DẪN THỰC HIỆN\n{guidance}");
        return Required(string.Join("\n\n", sections), "nội dung thông báo", 6000);
    }

    async Task ValidateSupplementResponse(string requestId, List<FileInput> documents, string? explanation, string? correction)
    {
        var requirements = await db.NoiDungBoSung.Where(x => x.YeuCauId == requestId).ToListAsync();
        if (requirements.Any(x => x.ThanhPhanId != null && !documents.Any(d => d.ThanhPhanId == x.ThanhPhanId)))
            throw new BusinessError("Cần gửi lại tệp cho từng thành phần được yêu cầu bổ sung.");
        if (requirements.Any(x => !string.IsNullOrWhiteSpace(x.TruongThongTin)) && string.IsNullOrWhiteSpace(correction))
            throw new BusinessError("Cần nhập đầy đủ nội dung đề nghị thay thế theo yêu cầu chỉnh sửa.");
        if (requirements.Any(x => x.ThanhPhanId == null && string.IsNullOrWhiteSpace(x.TruongThongTin)) && string.IsNullOrWhiteSpace(explanation))
            throw new BusinessError("Cần nhập giải trình cho nội dung bổ sung chung.");
        if (documents.Count == 0 && string.IsNullOrWhiteSpace(explanation) && string.IsNullOrWhiteSpace(correction))
            throw new BusinessError("Gửi tệp, giải trình hoặc nội dung điều chỉnh theo yêu cầu.");
    }

    public async Task<object> Act(string id, string action, ActionInput input, Actor a)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var h = await db.HoSo.FromSqlInterpolated($"SELECT * FROM [HoSo] WITH (UPDLOCK,ROWLOCK) WHERE HoSoId={id}").SingleOrDefaultAsync() ?? throw new BusinessError("Không tìm thấy hồ sơ.", 404);
        await LockVersion(h, a, input.Version);
        var before = h.TrangThai;
        var now = DateTime.UtcNow;
        string text = "";
        var open = await db.YeuCauBoSung.SingleOrDefaultAsync(x => x.HoSoId == id && x.DongLuc == null);
        switch (action)
        {
            case "nop":
                if (!a.Citizen || h.ChuHoSoId != a.Id || h.TrangThai != "BAN_NHAP")
                    throw new BusinessError("Chỉ nộp bản nháp của bạn.", 409);
                if (!await db.ThuTuc.FromSqlInterpolated($"SELECT * FROM [ThuTuc] WITH (HOLDLOCK,ROWLOCK) WHERE ThuTucId={h.ThuTucId}").AnyAsync(x => x.HoatDong))
                    throw new BusinessError("Thủ tục đã ngừng tiếp nhận.");
                Required(h.NoiDungDeNghi, "nội dung đề nghị");
                await CheckComponents(h, false);
                h.MaGui = Code("GUI");
                h.NgayNop = now;
                h.TrangThai = "DA_GUI";
                text = "Nộp hồ sơ, chờ cán bộ kiểm tra.";
                break;
            case "kiem-tra-tai-lieu":
                await Handler(h, a, h.TrangThai == "CHO_BO_SUNG" ? open?.TrangThaiQuayLai ?? "" : h.TrangThai);
                var document = await db.TaiLieuHoSo.SingleOrDefaultAsync(x => x.Id == input.TaiLieuId && x.HoSoId == id) ?? throw new BusinessError("Không tìm thấy tài liệu thuộc hồ sơ.");
                if (!document.HienHanh && !(open?.TrangThai == "DA_GUI" && document.LanId != null && await db.LanBoSung.AnyAsync(l => l.Id == document.LanId && l.YeuCauId == open.Id && l.DanhGia == null)))
                    throw new BusinessError("Chỉ kiểm tra bản hiện hành hoặc bản bổ sung đang chờ đánh giá.");
                if (input.Dat == null)
                    throw new BusinessError("Chọn Đạt hoặc Chưa đạt.");
                files.Get(document.TepKey);
                if (input.Dat == false)
                    Required(input.LyDo, "lý do chưa đạt");
                db.Add(new KiemTraThanhPhan { TaiLieuId = document.Id, CanBoId = a.Id, KetQua = input.Dat.Value ? "DAT" : "CHUA_DAT", NhanXet = input.Dat.Value ? input.NoiDung : input.LyDo, ThoiDiem = now });
                text = $"Đánh giá {document.TenTep} phiên bản {document.SoPhienBan}: {(input.Dat.Value ? "Đạt" : "Chưa đạt")}. {input.LyDo ?? input.NoiDung}";
                break;
            case "tiep-nhan":
                if (!a.Reception)
                    throw new BusinessError("Chỉ cán bộ tiếp nhận được thực hiện.", 403);
                if (before != "DA_GUI" || open != null)
                    throw new BusinessError("Chưa đủ điều kiện tiếp nhận.", 409);
                await CheckComponents(h, true);
                var proc = await db.ThuTuc.SingleAsync(x => x.Id == h.ThuTucId);
                sla.Capture(h, proc, now);
                h.MaHoSo = Code("HS");
                h.TrangThai = "DA_TIEP_NHAN";
                text = "Tiếp nhận hồ sơ, cấp mã chính thức, biên nhận và ngày hẹn trả.";
                break;
            case "luan-chuyen":
                if (!a.Reception)
                    throw new BusinessError("Cán bộ tiếp nhận phụ trách luân chuyển và phân công.", 403);
                if (before is not ("DA_TIEP_NHAN" or "DANG_GIAI_QUYET") || open != null)
                    throw new BusinessError("Chưa thể phân công ở trạng thái này.", 409);
                var officer = await db.NguoiDung.SingleOrDefaultAsync(x => x.Id == input.CanBoNhanId && x.HoatDong && x.VaiTro == "GIAI_QUYET") ?? throw new BusinessError("Cán bộ nhận không hợp lệ.");
                var department = await db.ThuTuc.Where(x => x.Id == h.ThuTucId).Select(x => x.BoPhanChuyenMonId).SingleAsync();
                if (officer.BoPhanId != department || input.BoPhanNhanId != department || !await db.BoPhan.AnyAsync(x => x.Id == department && x.HoatDong))
                    throw new BusinessError("Cán bộ nhận phải thuộc bộ phận chuyên môn của thủ tục.");
                text = Required(input.NoiDung, "nội dung bàn giao");
                await CloseAssignment(id);
                await db.SaveChangesAsync();
                db.Add(new PhanCongXuLy { HoSoId = id, NguoiChuyenId = a.Id, CanBoNhanId = officer.Id, BoPhanNhanId = department, BatDauLuc = now, NoiDungChuyen = text });
                h.TrangThai = "DANG_GIAI_QUYET";
                text = $"Giao {officer.HoTen}. {text}";
                break;
            case "yeu-cau-bo-sung":
                await Handler(h, a, before);
                if (open != null)
                    throw new BusinessError("Đã có yêu cầu chưa đóng.", 409);
                var items = input.NoiDungBoSung ?? [new(null, null, Required(input.NoiDung, "nội dung cần bổ sung"), Required(input.LyDo, "lý do"), Required(input.HuongDan, "hướng dẫn bổ sung"))];
                if (items.Count is < 1 or > 20)
                    throw new BusinessError("Cần từ 1 đến 20 nội dung bổ sung.");
                var request = new YeuCauBoSung
                {
                    HoSoId = id,
                    NguoiYeuCauId = a.Id,
                    TrangThai = "DANG_CHO",
                    TrangThaiQuayLai = before,
                    TaoLuc = now
                };
                db.Add(request);
                foreach (var item in items)
                {
                    if (item.ThanhPhanId != null && !await db.ThanhPhanYeuCau.AnyAsync(c => c.Id == item.ThanhPhanId && c.ThuTucId == h.ThuTucId))
                        throw new BusinessError("Thành phần yêu cầu không thuộc thủ tục.");
                    db.Add(new NoiDungBoSung { YeuCauId = request.Id, ThanhPhanId = item.ThanhPhanId, TruongThongTin = item.TruongThongTin, VanDe = Required(item.VanDe, "vấn đề"), LyDo = Required(item.LyDo, "lý do"), HuongDan = Required(item.HuongDan, "hướng dẫn") });
                }

                h.TrangThai = "CHO_BO_SUNG";
                text = "Yêu cầu bổ sung: " + string.Join("; ", items.Select(x => $"{x.VanDe}. Lý do: {x.LyDo}. Hướng dẫn: {x.HuongDan}"));
                break;
            case "gui-bo-sung":
                if (!a.Citizen || h.ChuHoSoId != a.Id)
                    throw new BusinessError("Chỉ chủ hồ sơ được gửi đáp ứng.", 403);
                if (before != "CHO_BO_SUNG" || open?.TrangThai != "DANG_CHO")
                    throw new BusinessError("Không có yêu cầu đang chờ gửi bổ sung.", 409);
                var rows = await ValidateFiles(input.TaiLieu, h.ThuTucId, a);
                await ValidateSupplementResponse(open.Id, rows, input.NoiDung, input.ThongTinDieuChinh);
                var submission = new LanBoSung
                {
                    YeuCauId = open.Id,
                    SoLan = (await db.LanBoSung.Where(x => x.YeuCauId == open.Id).MaxAsync(x => (int? )x.SoLan) ?? 0) + 1,
                    NoiDungGiaiTrinh = input.NoiDung,
                    ThongTinDieuChinh = input.ThongTinDieuChinh,
                    GuiLuc = now
                };
                db.Add(submission);
                await db.SaveChangesAsync();
                await AddFiles(h, a, rows, submission.Id, false);
                open.TrangThai = "DA_GUI";
                text = $"Gửi đáp ứng lần {submission.SoLan}, chờ cán bộ đánh giá. {input.NoiDung}";
                break;
            case "kiem-tra-bo-sung":
                if (before != "CHO_BO_SUNG" || open?.TrangThai != "DA_GUI")
                    throw new BusinessError("Không có lần bổ sung chờ kiểm tra.", 409);
                await Handler(h, a, open.TrangThaiQuayLai);
                if (input.Dat == null)
                    throw new BusinessError("Chọn kết quả đánh giá.");
                var latest = await db.LanBoSung.Where(x => x.YeuCauId == open.Id && x.DanhGia == null).OrderByDescending(x => x.SoLan).FirstOrDefaultAsync() ?? throw new BusinessError("Không tìm thấy lần gửi chờ đánh giá.");
                if (input.Dat.Value)
                {
                    var pending = await db.TaiLieuHoSo.Where(x => x.LanId == latest.Id).ToListAsync();
                    await ValidateSupplementResponse(open.Id, pending.Select(d => new FileInput(d.ThanhPhanId, d.TepKey)).ToList(), latest.NoiDungGiaiTrinh, latest.ThongTinDieuChinh);
                    foreach (var d in pending)
                    {
                        files.Get(d.TepKey);
                        var check = await db.KiemTraThanhPhan.Where(k => k.TaiLieuId == d.Id).OrderByDescending(k => k.ThoiDiem).FirstOrDefaultAsync();
                        if (check?.KetQua != "DAT")
                            throw new BusinessError("Cần đánh giá Đạt từng bản tệp bổ sung trước khi chấp nhận.");
                        var current = await db.TaiLieuHoSo.Where(x => x.HoSoId == id && x.ThanhPhanId == d.ThanhPhanId && x.HienHanh).ToListAsync();
                        current.ForEach(x => x.HienHanh = false);
                    }

                    await db.SaveChangesAsync();
                    pending.ForEach(x => x.HienHanh = true);
                    if (!string.IsNullOrWhiteSpace(latest.ThongTinDieuChinh))
                        h.NoiDungDeNghi = latest.ThongTinDieuChinh.Trim();
                    var oldDeadline = h.HanTra;
                    SlaService.Resume(h, open, latest);
                    open.DongLuc = now;
                    open.TrangThai = "DA_DAP_UNG";
                    h.TrangThai = open.TrangThaiQuayLai;
                    text = $"Bổ sung lần {latest.SoLan} đạt. Hạn trước: {oldDeadline:O}, hạn sau: {h.HanTra:O}.";
                }
                else
                {
                    open.TrangThai = "DANG_CHO";
                    text = "Bổ sung chưa đạt: " + Required(input.LyDo, "lý do chưa đạt");
                }

                latest.DanhGia = input.Dat.Value ? "DAT" : "CHUA_DAT";
                latest.NhanXet = input.Dat.Value ? input.NoiDung : input.LyDo;
                latest.NguoiDanhGiaId = a.Id;
                latest.DanhGiaLuc = now;
                break;
            case "ket-qua":
                await Handler(h, a, before);
                if (a.VaiTro != "GIAI_QUYET" || before != "DANG_GIAI_QUYET" || open != null)
                    throw new BusinessError("Chưa đủ điều kiện kết luận thành công.", 409);
                text = ResultNotice(input, true);
                if (string.IsNullOrWhiteSpace(input.TepKey))
                    throw new BusinessError("Cần tệp kết quả để phát hành điện tử.");
                files.Owned(input.TepKey, a.Sub);
                if (await db.TaiLieuHoSo.AnyAsync(x => x.TepKey == input.TepKey) || await db.KetQuaHoSo.AnyAsync(x => x.TepKey == input.TepKey))
                    throw new BusinessError("Tệp đã dùng cho hồ sơ khác.");
                db.Add(new KetQuaHoSo { HoSoId = id, LoaiKetQua = "THANH_CONG", NoiDungKetLuan = text, TepKey = input.TepKey, NguoiTaoId = a.Id, TaoLuc = now });
                h.TrangThai = "DA_CO_KET_QUA";
                await CloseAssignment(id);
                break;
            case "tu-choi":
                await Handler(h, a, before);
                if (open != null)
                    throw new BusinessError("Cần giải quyết yêu cầu bổ sung đang mở trước khi kết luận.", 409);
                text = ResultNotice(input, false);
                var reason = Required(input.LyDo, "lý do từ chối");
                h.TrangThai = before == "DA_GUI" ? "TU_CHOI_TIEP_NHAN" : "TU_CHOI_GIAI_QUYET";
                db.Add(new KetQuaHoSo { HoSoId = id, LoaiKetQua = h.TrangThai, NoiDungKetLuan = text, LyDoTuChoi = reason, NguoiTaoId = a.Id, NguoiPhatHanhId = a.Id, TaoLuc = now, PhatHanhLuc = now });
                if (h.NgayTiepNhan != null)
                    h.NgayKetThuc = now;
                await CloseAssignment(id);
                text += " Lý do: " + reason;
                break;
            case "tra-ket-qua":
                if (!a.Reception)
                    throw new BusinessError("Chỉ cán bộ tiếp nhận phát hành kết quả.", 403);
                if (before != "DA_CO_KET_QUA")
                    throw new BusinessError("Kết quả chưa sẵn sàng hoặc đã phát hành.", 409);
                var result = await db.KetQuaHoSo.SingleAsync(x => x.HoSoId == id);
                if (result.LoaiKetQua != "THANH_CONG" || result.PhatHanhLuc != null || result.TepKey == null || !File.Exists(files.PathFor(result.TepKey)))
                    throw new BusinessError("Tệp kết quả chưa sẵn sàng để phát hành.");
                result.NguoiPhatHanhId = a.Id;
                result.PhatHanhLuc = now;
                h.NgayKetThuc = now;
                h.TrangThai = "DA_TRA_KET_QUA";
                text = "Phát hành kết quả điện tử. " + input.NoiDung;
                break;
            default:
                throw new BusinessError("Thao tác không tồn tại.", 404);
        }

        h.RowVersion++;
        History(h, a, action.Replace('-', '_').ToUpperInvariant(), before, text);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Summary(h);
    }
}
