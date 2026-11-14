namespace CongMotCua.Api.Models;
public class YeuCauBoSung : Entity
{
    public string HoSoId { get; set; } = "";
    public string NguoiYeuCauId { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public string TrangThaiQuayLai { get; set; } = "";
    public DateTime TaoLuc { get; set; }
    public DateTime? DongLuc { get; set; }
}
