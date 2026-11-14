using CongMotCua.Api.Models;
using CongMotCua.Api.Models.Requests;
using CongMotCua.Api.Filters;
using CongMotCua.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CongMotCua.Api.Models.Data;
public static class SeedData
{
    public static async Task Run(AppDb db, FileStore files, string issuer)
    {
        if (await db.ThuTuc.AnyAsync())
            return;
        await using var tx = await db.Database.BeginTransactionAsync();
        var receive = new BoPhan
        {
            Id = "bp-tiep-nhan",
            MaBoPhan = "TN",
            TenBoPhan = "Tiếp nhận và trả kết quả"
        };
        var professional = new BoPhan
        {
            Id = "bp-chuyen-mon",
            MaBoPhan = "CM",
            TenBoPhan = "Bộ phận chuyên môn"
        };
        db.AddRange(receive, professional);
        var rows = new[]
        {
            ("11111111-1111-4111-8111-111111111111", "minhanh", "Nguyễn Minh Anh", "NGUOI_NOP"),
            ("22222222-2222-4222-8222-222222222222", "hoangnam", "Trần Hoàng Nam", "NGUOI_NOP"),
            ("33333333-3333-4333-8333-333333333333", "tiepnhan", "Lê Thu Hà", "TIEP_NHAN"),
            ("44444444-4444-4444-8444-444444444444", "giaiquyet", "Phạm Đức Huy", "GIAI_QUYET"),
            ("55555555-5555-4555-8555-555555555555", "ngoclinh", "Võ Ngọc Linh", "GIAI_QUYET")
        };
        foreach (var(sub, name, full, role)in rows)
            db.Add(new NguoiDung { Id = name, Issuer = issuer, Subject = sub, HoTen = full, Email = name + "@example.test", VaiTro = role, BoPhanId = role == "NGUOI_NOP" ? null : role == "TIEP_NHAN" ? receive.Id : professional.Id });
        var names = new[]
        {
            "Cấp bản sao trích lục hộ tịch",
            "Chứng thực bản sao từ bản chính",
            "Xác nhận thông tin theo đề nghị"
        };
        for (int i = 0; i < 3; i++)
        {
            var proc = new ThuTuc
            {
                Id = $"tt-{i + 1}",
                MaThuTuc = $"DEMO-0{i + 1}",
                TenThuTuc = names[i],
                MoTa = "Thủ tục minh họa cho đồ án. Chuẩn bị đơn đề nghị và giấy tờ chứng minh, nộp trực tuyến và theo dõi ngày hẹn trả sau tiếp nhận.",
                SoNgaySLA = i + 3,
                DonViSLA = "NGAY_LAM_VIEC",
                SoGioPhanHoi = 8,
                QuyTacBoSung = "TINH_LAI",
                BoPhanChuyenMonId = professional.Id
            };
            db.Add(proc);
            db.AddRange(new ThanhPhanYeuCau { Id = $"tp-{i + 1}-1", ThuTucId = proc.Id, TenThanhPhan = "Đơn đề nghị", BatBuoc = true, HuongDan = "Bản PDF, ảnh chụp rõ hoặc tệp TXT mẫu để trình diễn.", ThuTu = 1 }, new ThanhPhanYeuCau { Id = $"tp-{i + 1}-2", ThuTucId = proc.Id, TenThanhPhan = "Giấy tờ chứng minh liên quan", BatBuoc = true, HuongDan = "Đính kèm đầy đủ giấy tờ theo nội dung đề nghị.", ThuTu = 2 });
        }

        await db.SaveChangesAsync();
        var sla = new SlaService(new ConfigurationBuilder().Build());
        for (int i = 0; i < 6; i++)
        {
            var proc = await db.ThuTuc.SingleAsync(x => x.Id == $"tt-{i % 3 + 1}");
            var citizen = rows[i == 5 ? 1 : 0];
            var now = DateTime.UtcNow.AddDays(-i - 1);
            var h = new HoSo
            {
                Id = $"demo-hs-{i + 1}",
                ThuTucId = proc.Id,
                ChuHoSoId = citizen.Item2,
                NoiDungDeNghi = $"Hồ sơ minh họa {i + 1}: {proc.TenThuTuc}",
                TrangThai = i == 0 ? "BAN_NHAP" : "DA_GUI",
                MaGui = i == 0 ? null : $"GUI-DEMO-{i + 1:000}",
                NgayNop = i == 0 ? null : now,
                RowVersion = 1
            };
            db.Add(h);
            await db.SaveChangesAsync();
            foreach (var tp in await db.ThanhPhanYeuCau.Where(x => x.ThuTucId == proc.Id).ToListAsync())
            {
                var f = await files.Write(tp.TenThanhPhan + ".txt", "text/plain", citizen.Item1, Encoding.UTF8.GetBytes($"TÀI LIỆU MẪU ĐỒ ÁN\n{tp.TenThanhPhan}\n{h.NoiDungDeNghi}\nKhông phải giấy tờ thật."));
                var doc = new TaiLieuHoSo
                {
                    HoSoId = h.Id,
                    ThanhPhanId = tp.Id,
                    TenTep = f.Name,
                    TepKey = f.Key,
                    LoaiTep = f.ContentType,
                    DungLuong = (int)f.Size,
                    SoPhienBan = 1,
                    HienHanh = true,
                    TaoLuc = now
                };
                db.Add(doc);
                if (i >= 2)
                    db.Add(new KiemTraThanhPhan { TaiLieuId = doc.Id, CanBoId = "tiepnhan", KetQua = "DAT", ThoiDiem = now.AddHours(1) });
            }

            db.Add(new LichSuXuLy { HoSoId = h.Id, NguoiThucHienId = citizen.Item2, HanhDong = i == 0 ? "LUU_NHAP" : "NOP_HO_SO", TrangThaiTruoc = "BAN_NHAP", TrangThaiSau = h.TrangThai, NoiDung = "Dữ liệu mẫu phục vụ trình diễn đồ án.", ThoiDiem = now });
            if (i >= 2)
            {
                sla.Capture(h, proc, now.AddHours(2));
                h.MaHoSo = $"HS-DEMO-{i + 1:000}";
                h.TrangThai = "DA_TIEP_NHAN";
                db.Add(new LichSuXuLy { HoSoId = h.Id, NguoiThucHienId = "tiepnhan", HanhDong = "TIEP_NHAN", TrangThaiTruoc = "DA_GUI", TrangThaiSau = h.TrangThai, NoiDung = "Tiếp nhận và cấp ngày hẹn trả.", ThoiDiem = now.AddHours(2) });
            }

            if (i >= 3)
            {
                h.TrangThai = "DANG_GIAI_QUYET";
                db.Add(new PhanCongXuLy { HoSoId = h.Id, NguoiChuyenId = "tiepnhan", CanBoNhanId = "giaiquyet", BoPhanNhanId = professional.Id, BatDauLuc = now.AddHours(3), KetThucLuc = i >= 4 ? now.AddHours(4) : null, NoiDungChuyen = "Phân công mẫu." });
                db.Add(new LichSuXuLy { HoSoId = h.Id, NguoiThucHienId = "tiepnhan", HanhDong = "PHAN_CONG", TrangThaiTruoc = "DA_TIEP_NHAN", TrangThaiSau = h.TrangThai, NoiDung = "Giao cán bộ chuyên môn.", ThoiDiem = now.AddHours(3) });
            }

            if (i >= 4)
            {
                var f = await files.Write("ket-qua-mau.txt", "text/plain", rows[3].Item1, Encoding.UTF8.GetBytes("KẾT QUẢ MẪU PHỤC VỤ ĐỒ ÁN\nNội dung đề nghị đã được xem xét.\nKhông có giá trị pháp lý."));
                h.TrangThai = i == 4 ? "DA_CO_KET_QUA" : "DA_TRA_KET_QUA";
                db.Add(new KetQuaHoSo { HoSoId = h.Id, LoaiKetQua = "THANH_CONG", NoiDungKetLuan = "Đồng ý giải quyết nội dung đề nghị theo hồ sơ mẫu.", TepKey = f.Key, NguoiTaoId = "giaiquyet", TaoLuc = now.AddHours(4), NguoiPhatHanhId = i == 5 ? "tiepnhan" : null, PhatHanhLuc = i == 5 ? now.AddHours(5) : null });
                if (i == 5)
                    h.NgayKetThuc = now.AddHours(5);
                db.Add(new LichSuXuLy { HoSoId = h.Id, NguoiThucHienId = "giaiquyet", HanhDong = "KET_QUA", TrangThaiTruoc = "DANG_GIAI_QUYET", TrangThaiSau = "DA_CO_KET_QUA", NoiDung = "Lập kết quả, chờ phát hành.", ThoiDiem = now.AddHours(4) });
                if (i == 5)
                    db.Add(new LichSuXuLy { HoSoId = h.Id, NguoiThucHienId = "tiepnhan", HanhDong = "TRA_KET_QUA", TrangThaiTruoc = "DA_CO_KET_QUA", TrangThaiSau = "DA_TRA_KET_QUA", NoiDung = "Phát hành kết quả mẫu.", ThoiDiem = now.AddHours(5) });
            }
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
