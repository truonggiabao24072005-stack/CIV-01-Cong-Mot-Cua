namespace CongMotCua.Api.Models;
public class PhanCongXuLy : Entity
{
    public string HoSoId { get; set; } = "";
    public string NguoiChuyenId { get; set; } = "";
    public string CanBoNhanId { get; set; } = "";
    public string BoPhanNhanId { get; set; } = "";
    public DateTime BatDauLuc { get; set; }
    public DateTime? KetThucLuc { get; set; }
    public string NoiDungChuyen { get; set; } = "";
}
