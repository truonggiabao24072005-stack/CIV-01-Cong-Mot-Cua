namespace CongMotCua.Api.Models;
public class LanBoSung : Entity
{
    public string YeuCauId { get; set; } = "";
    public int SoLan { get; set; }
    public string? NoiDungGiaiTrinh { get; set; }
    public string? ThongTinDieuChinh { get; set; }
    public DateTime GuiLuc { get; set; }
    public string? DanhGia { get; set; }
    public string? NhanXet { get; set; }
    public string? NguoiDanhGiaId { get; set; }
    public DateTime? DanhGiaLuc { get; set; }
}
