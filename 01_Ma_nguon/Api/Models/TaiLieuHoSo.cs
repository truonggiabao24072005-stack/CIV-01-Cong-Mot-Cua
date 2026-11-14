namespace CongMotCua.Api.Models;
public class TaiLieuHoSo : Entity
{
    public string HoSoId { get; set; } = "";
    public string ThanhPhanId { get; set; } = "";
    public string? LanId { get; set; }
    public int SoPhienBan { get; set; }
    public string TenTep { get; set; } = "";
    public string LoaiTep { get; set; } = "";
    public int DungLuong { get; set; }
    public string TepKey { get; set; } = "";
    public bool HienHanh { get; set; }
    public DateTime TaoLuc { get; set; }
}
