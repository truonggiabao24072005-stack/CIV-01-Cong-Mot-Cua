namespace CongMotCua.Api.Models;
public class NoiDungBoSung : Entity
{
    public string YeuCauId { get; set; } = "";
    public string? ThanhPhanId { get; set; }
    public string? TruongThongTin { get; set; }
    public string VanDe { get; set; } = "";
    public string LyDo { get; set; } = "";
    public string HuongDan { get; set; } = "";
}
