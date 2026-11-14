using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CongMotCua.Api.Filters;
public sealed class BackendOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? "";
        var tag = path switch
        {
            _ when path.StartsWith("api/auth") || path == "api/me" => "01. Đăng nhập và tài khoản",
            _ when path.StartsWith("api/thu-tuc") => "02. Tra cứu thủ tục",
            _ when path.StartsWith("api/tra-cuu") => "03. Tra cứu công khai",
            _ when path.StartsWith("api/quan-tri") => "07. Quản lý danh mục",
            _ when path.StartsWith("api/bao-cao") || path == "api/thong-ke" => "08. Thống kê",
            _ when path.StartsWith("api/tep") || path.StartsWith("api/tai-lieu") || path.EndsWith("tep-ket-qua") => "06. Tệp hồ sơ và kết quả",
            _ when path.StartsWith("api/ho-so") => "04. Hồ sơ và quy trình xử lý",
            _ when path == "api/can-bo" => "05. Danh sách cán bộ",
            _ => "00. Trạng thái hệ thống"
        };
        operation.Tags = [new OpenApiTag
        {
            Name = tag
        }

        ];
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
            operation.Security = [new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
            }

            ];
        if (path.EndsWith("{thaoTac}"))
            operation.Description = "Các action: nop, kiem-tra-tai-lieu, tiep-nhan, luan-chuyen, yeu-cau-bo-sung, gui-bo-sung, kiem-tra-bo-sung, ket-qua, tu-choi, tra-ket-qua. Cần version hiện tại từ GET chi tiết hồ sơ. Vai trò và trạng thái được kiểm tra ở WorkflowService. Xem docs/API.md để biết trường bắt buộc của từng thao tác.";
        if (path == "api/tep" && context.ApiDescription.HttpMethod == "POST")
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Required = new HashSet<string>
                            {
                                "file"
                            },
                            Properties =
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            }
                        }
                    }
                }
            };
    }
}
