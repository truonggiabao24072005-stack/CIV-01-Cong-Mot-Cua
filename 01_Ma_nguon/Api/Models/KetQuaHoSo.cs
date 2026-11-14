namespace CongMotCua.Api.Models;
public class KetQuaHoSo : Entity
{
    public string HoSoId { get; set; } = "";
    public string LoaiKetQua { get; set; } = "";
    public string NoiDungKetLuan { get; set; } = "";
    public string? LyDoTuChoi { get; set; }
    public string? TepKey { get; set; }
    public string NguoiTaoId { get; set; } = "";
    public DateTime TaoLuc { get; set; }
    public string? NguoiPhatHanhId { get; set; }
    public DateTime? PhatHanhLuc { get; set; }
}
