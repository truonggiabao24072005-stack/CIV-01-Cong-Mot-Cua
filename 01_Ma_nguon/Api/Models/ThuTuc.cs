namespace CongMotCua.Api.Models;
public class ThuTuc : Entity
{
    public string MaThuTuc { get; set; } = "";
    public string TenThuTuc { get; set; } = "";
    public string MoTa { get; set; } = "";
    public int SoNgaySLA { get; set; }
    public string DonViSLA { get; set; } = "";
    public int SoGioPhanHoi { get; set; }
    public string QuyTacBoSung { get; set; } = "";
    public string BoPhanChuyenMonId { get; set; } = "";
    public bool HoatDong { get; set; } = true;
}
