namespace CongMotCua.Api.Models.Requests;
public record DepartmentInput(string? Id, string Ma, string Ten, bool HoatDong);
public record ComponentInput(string Ten, bool BatBuoc, string HuongDan);
public record ProcedureInput(string? Id, string Ma, string Ten, string MoTa, int SoNgaySLA, string DonViSLA, int SoGioPhanHoi, string QuyTacBoSung, string BoPhanId, bool HoatDong, List<ComponentInput> ThanhPhan);
public record UserAdminInput(string Id, bool HoatDong);
