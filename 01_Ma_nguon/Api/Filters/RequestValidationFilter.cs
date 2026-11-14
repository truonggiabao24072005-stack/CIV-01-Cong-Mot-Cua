using CongMotCua.Api.Models.Requests;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CongMotCua.Api.Filters;
public sealed class RequestValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid || context.ActionDescriptor.Parameters.Any(parameter =>
                parameter.BindingInfo?.BindingSource == BindingSource.Body &&
                (!context.ActionArguments.TryGetValue(parameter.Name, out var body) || body is null)))
            throw new BusinessError("Dữ liệu yêu cầu không đúng định dạng.");

        Text(context.HttpContext.Request.Query["q"].FirstOrDefault(), "Từ khóa", 200);
        foreach (var value in context.ActionArguments.Values)
        {
            switch (value)
            {
                case LoginInput input:
                    Text(input.Username, "Tên đăng nhập", 40, true);
                    Text(input.Password, "Mật khẩu", 128, true);
                    break;
                case RegisterInput input:
                    Text(input.Username, "Tên đăng nhập", 40, true);
                    Text(input.Password, "Mật khẩu", 128, true);
                    Text(input.HoTen, "Họ tên", 100, true);
                    Text(input.Email, "Email", 255, true);
                    break;
                case DraftInput input:
                    Text(input.ThuTucId, "Thủ tục", 50, true);
                    Text(input.NoiDung, "Nội dung đề nghị", 6000);
                    Files(input.TaiLieu);
                    Text(input.ThongTinLienHe?.SoDienThoai, "Số điện thoại", 30);
                    Text(input.ThongTinLienHe?.DiaChi, "Địa chỉ", 500);
                    break;
                case ActionInput input:
                    if (input.Version < 1)
                        throw new BusinessError("Phiên bản hồ sơ không hợp lệ.");
                    Text(input.NoiDung, "Nội dung", 6000);
                    Text(input.LyDo, "Lý do", 6000);
                    Text(input.HuongDan, "Hướng dẫn", 6000);
                    Text(input.CanCuXuLy, "Căn cứ kiểm tra", 1600);
                    Text(input.KetQuaCuThe, "Kết quả cụ thể", 1600);
                    Text(input.HuongDanThucHien, "Hướng dẫn thực hiện", 1200);
                    Text(input.ThongTinDieuChinh, "Thông tin điều chỉnh", 6000);
                    Text(input.CanBoNhanId, "Cán bộ nhận", 50);
                    Text(input.BoPhanNhanId, "Bộ phận nhận", 50);
                    Text(input.TaiLieuId, "Tài liệu", 50);
                    Text(input.TepKey, "Mã tệp", 32);
                    Files(input.TaiLieu);
                    if (input.NoiDungBoSung?.Count > 20)
                        throw new BusinessError("Tối đa 20 mục bổ sung.");
                    foreach (var item in input.NoiDungBoSung ?? [])
                    {
                        if (item is null)
                            throw new BusinessError("Mục bổ sung không hợp lệ.");
                        Text(item.ThanhPhanId, "Thành phần", 50);
                        Text(item.TruongThongTin, "Trường thông tin", 255);
                        Text(item.VanDe, "Vấn đề", 6000, true);
                        Text(item.LyDo, "Lý do", 6000, true);
                        Text(item.HuongDan, "Hướng dẫn", 6000, true);
                    }

                    break;
                case PublicLookup input:
                    Text(input.Code, "Mã hồ sơ", 50, true);
                    Text(input.ChallengeId, "Mã xác nhận", 32, true);
                    Text(input.Answer, "Câu trả lời", 10, true);
                    break;
                case ProfileInput input:
                    Text(input.SoDienThoai, "Số điện thoại", 30);
                    break;
                case DepartmentInput input:
                    Text(input.Id, "Mã định danh", 50);
                    Text(input.Ma, "Mã bộ phận", 255, true);
                    Text(input.Ten, "Tên bộ phận", 255, true);
                    break;
                case ProcedureInput input:
                    Text(input.Id, "Mã định danh", 50);
                    Text(input.Ma, "Mã thủ tục", 255, true);
                    Text(input.Ten, "Tên thủ tục", 255, true);
                    Text(input.MoTa, "Mô tả", 6000, true);
                    Text(input.BoPhanId, "Bộ phận", 50, true);
                    if (input.ThanhPhan is null || input.ThanhPhan.Count is < 1 or > 30)
                        throw new BusinessError("Thủ tục cần từ 1 đến 30 thành phần.");
                    foreach (var item in input.ThanhPhan)
                    {
                        if (item is null)
                            throw new BusinessError("Thành phần không hợp lệ.");
                        Text(item.Ten, "Tên thành phần", 255, true);
                        Text(item.HuongDan, "Hướng dẫn thành phần", 6000, true);
                    }

                    break;
                case UserAdminInput input:
                    Text(input.Id, "Người dùng", 50, true);
                    break;
            }
        }

    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private static void Text(string? value, string name, int maximum, bool required = false)
    {
        if (required && string.IsNullOrWhiteSpace(value))
            throw new BusinessError($"Vui lòng nhập {name.ToLowerInvariant()}.");
        if (value?.Length > maximum)
            throw new BusinessError($"{name} tối đa {maximum} ký tự.");
    }

    private static void Files(List<FileInput>? files)
    {
        if (files?.Count > 30)
            throw new BusinessError("Tối đa 30 tài liệu trong một lần gửi.");
        foreach (var file in files ?? [])
        {
            if (file is null)
                throw new BusinessError("Tài liệu không hợp lệ.");
            Text(file.ThanhPhanId, "Thành phần", 50, true);
            Text(file.TepKey, "Mã tệp", 32, true);
        }
    }
}
