namespace CongMotCua.Api.Models;
public class LichSuXuLy : Entity
{
    public string HoSoId { get; set; } = "";
    public string NguoiThucHienId { get; set; } = "";
    public string HanhDong { get; set; } = "";
    public string TrangThaiTruoc { get; set; } = "";
    public string TrangThaiSau { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public bool CongKhai { get; set; } = true;
    public DateTime ThoiDiem { get; set; }
}
