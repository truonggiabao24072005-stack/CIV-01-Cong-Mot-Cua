namespace CongMotCua.Api.Models;
public class HoSo : Entity
{
    public string? MaGui { get; set; }
    public string? MaHoSo { get; set; }
    public string ChuHoSoId { get; set; } = "";
    public string ThuTucId { get; set; } = "";
    public string? ThongTinNguoiNop { get; set; }
    public string? NoiDungDeNghi { get; set; }
    public string TrangThai { get; set; } = "";
    public DateTime? NgayNop { get; set; }
    public DateTime? NgayTiepNhan { get; set; }
    public DateTime? MocTinhSLA { get; set; }
    public int? SoNgaySLA { get; set; }
    public string? DonViSLA { get; set; }
    public string? QuyTacBoSung { get; set; }
    public string? LichApDung { get; set; }
    public DateTime? HanTraGoc { get; set; }
    public DateTime? HanTra { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public int RowVersion { get; set; } = 1;
}
