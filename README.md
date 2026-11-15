# Đồ án cuối kỳ CIV-01 — Cổng một cửa theo dõi hồ sơ dịch vụ công

- **Sinh viên** Trương Gia Bảo
- **Mã sinh viên** 1250080020
- **Lớp** 12_ĐH_CNPM1
- **Trường** Đại học Tài nguyên và Môi trường TP.HCM
- **Học phần** Phương pháp phát triển phần mềm hướng đối tượng
- **Giảng viên hướng dẫn** ThS. Phạm Trọng Huynh

## 1. Môi trường phát triển

- Giao diện dùng HTML, CSS và JavaScript.
- API dùng ASP.NET Core trên .NET 9. Entity Framework Core kết nối Microsoft SQL Server.
- Chạy dự án bằng Visual Studio Code và PowerShell trên Windows. Có thể dùng SSMS để xem và quản trị cơ sở dữ liệu.
- Sơ đồ được vẽ bằng Draw.io. Các màn hình trong báo cáo được phác thảo bằng Figma.

## 2. Nội dung đã thực hiện

Hệ thống quản lý hồ sơ từ lúc người dân lưu nháp hoặc nộp đến khi trả kết quả. Hồ sơ có thể quay lại bước bổ sung trước hoặc sau tiếp nhận, tùy giai đoạn phát sinh yêu cầu.

| Tệp và thư mục | Nội dung |
| --- | --- |
| [Mã nguồn](01_Ma_nguon/) | [Giao diện web](01_Ma_nguon/wwwroot/), [API](01_Ma_nguon/Api/) và [dịch vụ nghiệp vụ](01_Ma_nguon/Services/) |
| [Cơ sở dữ liệu](01_Ma_nguon/sql/) | Tập lệnh tạo 13 bảng, ràng buộc và quản trị SQL Server |
| [Bảng biểu](02_Bang_bieu/bang-bieu/) | Tệp CSV theo số bảng trong báo cáo |
| [Sơ đồ Draw.io](03_So_do_Drawio/) | 37 sơ đồ, gồm Use Case, lớp, trạng thái, tuần tự, hoạt động, ERD và kiến trúc |
| [Thiết kế Figma](04_Giao_dien_Figma/) | 20 màn hình của Chương VI, mỗi màn hình một tệp `.fig` |

## 3. Kết quả đạt được

| Vai trò | Công việc trên hệ thống |
| --- | --- |
| Người dân | Xem giấy tờ cần chuẩn bị, lưu nháp, nộp hồ sơ, gửi bổ sung, theo dõi tiến độ và tải kết quả đã phát hành |
| Cán bộ tiếp nhận | Kiểm tra giấy tờ, tiếp nhận hoặc từ chối, phân công chuyên môn, phát hành kết quả và xem báo cáo |
| Cán bộ chuyên môn | Xử lý hồ sơ được giao, kiểm tra đáp ứng bổ sung và ghi kết quả xử lý |

Bộ dữ liệu thực hành dùng khi trình bày có 1.080 hồ sơ ở chín trạng thái. Bàn làm việc hỗ trợ mở danh sách theo trạng thái, tra hồ sơ theo thủ tục và hạn trả, xem yêu cầu bổ sung và lọc báo cáo theo ngày nộp.

## 4. Vấn đề và cách xử lý

- **Mở lại hồ sơ sau khi có cập nhật.** API kiểm tra số phiên bản trước khi ghi. Khi phiên bản đã thay đổi, giao diện yêu cầu tải lại hồ sơ.
- **Bổ sung hồ sơ ở nhiều giai đoạn.** Yêu cầu bổ sung ghi lại giai đoạn cần quay về. Kết luận đánh giá quyết định hồ sơ trở lại giai đoạn cũ hay tiếp tục chờ bổ sung.
- **Quyền xem tài liệu và kết quả.** API kiểm tra vai trò, chủ hồ sơ và trạng thái khi mở tệp. Người dân tải được kết quả điện tử của hồ sơ mình sau khi phát hành.

## 5. Hướng dẫn chạy chương trình

### Bước 1. Chuẩn bị

Cài .NET SDK 9 và SQL Server. Bật SQL Server, rồi mở thư mục `01_Ma_nguon` trong VS Code.

### Bước 2. Khởi tạo cơ sở dữ liệu

Mở Terminal PowerShell tại thư mục chứa `CongMotCua.csproj` và chạy một lần.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup.ps1
```

Script tạo cấu hình riêng trên máy và khởi tạo cơ sở dữ liệu `CongMotCua_VanHanh`. Với SQL Server dùng instance tên khác, thêm tham số `-Server`, ví dụ `-Server '.\SQLEXPRESS'`.

Dùng [hướng dẫn cấp tài khoản cán bộ](01_Ma_nguon/README.md#cài-đặt-và-chạy-bằng-vs-code) để tạo tài khoản tiếp nhận. [Tập lệnh danh mục khởi đầu](01_Ma_nguon/sql/06_Danh_muc_khoi_dau.sql) có thể dùng để thêm ba thủ tục cho buổi trình bày.

### Bước 3. Chạy trong VS Code

```powershell
dotnet watch run
```

Mở [Cổng một cửa trên máy](http://localhost:5080/). Người dân đăng ký tài khoản trên web. Xem thêm [tài liệu API](01_Ma_nguon/docs/API.md) và [hướng dẫn quản trị dữ liệu](01_Ma_nguon/docs/QUAN_TRI_SQL_SERVER.md).
