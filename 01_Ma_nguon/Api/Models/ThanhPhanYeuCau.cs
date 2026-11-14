namespace CongMotCua.Api.Models;
public class ThanhPhanYeuCau : Entity
{
    public string ThuTucId { get; set; } = "";
    public string TenThanhPhan { get; set; } = "";
    public bool BatBuoc { get; set; }
    public string HuongDan { get; set; } = "";
    public int ThuTu { get; set; }
}
