namespace CongMotCua.Api.Models;
public class NguoiDung : Entity
{
    public string Issuer { get; set; } = "";
    public string Subject { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? BoPhanId { get; set; }
    public bool HoatDong { get; set; } = true;
    public string VaiTro { get; set; } = "";
}
