namespace CongMotCua.Api.Models;
public class BoPhan : Entity
{
    public string MaBoPhan { get; set; } = "";
    public string TenBoPhan { get; set; } = "";
    public bool HoatDong { get; set; } = true;
}
