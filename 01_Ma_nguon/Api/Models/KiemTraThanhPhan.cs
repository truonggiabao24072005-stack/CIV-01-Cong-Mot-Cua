namespace CongMotCua.Api.Models;
public class KiemTraThanhPhan : Entity
{
    public string TaiLieuId { get; set; } = "";
    public string CanBoId { get; set; } = "";
    public string KetQua { get; set; } = "";
    public string? NhanXet { get; set; }
    public DateTime ThoiDiem { get; set; }
}
