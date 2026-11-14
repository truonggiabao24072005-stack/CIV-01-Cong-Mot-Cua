namespace CongMotCua.Api.Models.Requests;
public record FileInput(string ThanhPhanId, string TepKey);
public record ContactInput(string? SoDienThoai, string? DiaChi);
public record DraftInput(string ThuTucId, string? NoiDung, List<FileInput>? TaiLieu, bool Nop = false, ContactInput? ThongTinLienHe = null);
public record SupplementItem(string? ThanhPhanId, string? TruongThongTin, string VanDe, string LyDo, string HuongDan);
public record ActionInput(
    int Version,
    string? NoiDung = null,
    string? LyDo = null,
    string? CanBoNhanId = null,
    string? BoPhanNhanId = null,
    bool? Dat = null,
    string? TepKey = null,
    List<FileInput>? TaiLieu = null,
    string? TaiLieuId = null,
    string? HuongDan = null,
    List<SupplementItem>? NoiDungBoSung = null,
    string? ThongTinDieuChinh = null,
    string? CanCuXuLy = null,
    string? KetQuaCuThe = null,
    string? HuongDanThucHien = null);
public record Actor(string Sub, string Ten, string VaiTro, string Id, string? BoPhanId, string? Email = null, string? SoDienThoai = null)
{
    public bool Citizen => VaiTro == "NGUOI_NOP";
    public bool Reception => VaiTro == "TIEP_NHAN";
}
