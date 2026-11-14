# Hệ thống Cổng một cửa

Hệ thống hỗ trợ nộp hồ sơ trực tuyến, tiếp nhận, phân công, yêu cầu bổ sung, xử lý và trả kết quả điện tử.

Sinh viên: **Trương Gia Bảo – 1250080020**.

## Chức năng chính

- **Người dân:** tạo và nộp hồ sơ, gửi bổ sung, theo dõi tiến độ, nhận kết quả.
- **Cán bộ tiếp nhận:** kiểm tra hồ sơ, tiếp nhận hoặc từ chối, phân công xử lý, phát hành kết quả.
- **Cán bộ chuyên môn:** kiểm tra tài liệu, yêu cầu bổ sung, ghi nhận kết quả xử lý.
- Danh mục thủ tục, thành phần hồ sơ, bộ phận; lịch sử xử lý; thống kê; biểu mẫu in.

Bản cài mới tạo cơ sở dữ liệu trống. Đơn vị sử dụng tự cấu hình tài khoản cán bộ, bộ phận, thủ tục và thời hạn. VNeID, chữ ký số và gửi thông báo qua email hoặc SMS chưa được tích hợp.

## Công nghệ và cấu trúc

ASP.NET Core trên .NET 9, Entity Framework Core, SQL Server; giao diện HTML, CSS và JavaScript.

| Thư mục | Nội dung |
|---|---|
| `Api/Controllers` | Nhận yêu cầu từ giao diện |
| `Api/Models` | Mô hình dữ liệu và dữ liệu gửi lên |
| `Services` | Quy tắc nghiệp vụ, phân quyền, lưu tệp và danh tính cục bộ |
| `wwwroot` | Giao diện HTML, CSS, JavaScript và tài nguyên tĩnh |
| `sql` | Tạo 13 bảng, ràng buộc, quản trị và dữ liệu mẫu |
| `scripts` | Khởi tạo, chạy, dừng và hỗ trợ quản trị SQL Server |
| `keycloak` | Cấu hình SSO tùy chọn, không cần để chạy mặc định |

## Cài đặt và chạy bằng VS Code

Mở thư mục chứa `CongMotCua.csproj` trong VS Code. Sau khi cấu hình và khởi tạo SQL Server, chạy `dotnet watch run` trong Terminal PowerShell.

Cần Windows, **.NET SDK 9**, **SQL Server** đang chạy và tài khoản Windows có quyền tạo cơ sở dữ liệu. Có thể dùng SSMS để xem và quản trị dữ liệu.

Mở đúng thư mục chứa `CongMotCua.csproj` trong VS Code. Tại Terminal PowerShell, chạy lần đầu:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup.ps1
```

Nếu SQL Server có tên instance khác, truyền tên instance, ví dụ:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup.ps1 -Server '.\SQLEXPRESS'
```

Script tạo cấu hình riêng, cơ sở dữ liệu `CongMotCua_VanHanh` và bản chạy. Trước khi mở web, cấp tài khoản tiếp nhận từ máy chủ. Thay các giá trị bên dưới bằng thông tin của cán bộ được phân quyền. Script sẽ hỏi mật khẩu trong cửa sổ lệnh.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\cap-tai-khoan.ps1 -TenDangNhap ten.canbo -HoTen 'Họ tên cán bộ' -Email 'canbo@example.org' -VaiTro TIEP_NHAN
```

Sau đó chạy:

```powershell
dotnet watch run
```

Mở **http://localhost:5080**. Tài liệu API trong môi trường phát triển: **http://localhost:5080/swagger**.

Cách chạy bản đã biên dịch:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\start.ps1
```

Dừng bản chạy nền:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\stop.ps1
```

### Thiết lập danh mục và tài khoản

1. Đăng nhập với tài khoản tiếp nhận vừa cấp. Trong **Quản lý danh mục**, tạo bộ phận chuyên môn, thủ tục, thành phần hồ sơ và thời hạn đã được đơn vị phê duyệt.
2. Dừng web. Cấp tài khoản chuyên môn bằng `cap-tai-khoan.ps1` với `-VaiTro GIAI_QUYET -MaBoPhan <mã-bộ-phận>` rồi khởi động lại. Mỗi cán bộ có mật khẩu riêng.
3. Người dân đăng ký tài khoản trên web. Giữ `appsettings.Local.json` và thư mục `.data` trên máy phục vụ để duy trì tài khoản và kho tệp.

Khi cập nhật bản chạy, dừng web rồi chạy `setup.ps1 -Build`. Nếu đã có cấu hình cục bộ, lệnh giữ nguyên cơ sở dữ liệu, kho tệp và kho tài khoản. Chỉ truyền `-Database` hoặc `-Server` khi chủ động cấu hình lại môi trường.

### SQL Server

- Dùng script khởi tạo như hướng dẫn trên để chạy ứng dụng lần đầu.
- [Tập lệnh SQL tổng hợp](sql/CongMotCua_HoanChinh.sql) dành cho việc đọc, đối chiếu hoặc tạo một cơ sở dữ liệu mới bằng SSMS; không chạy lại phần tạo bảng trên cơ sở dữ liệu đã có bảng.
- [Hướng dẫn quản trị](docs/QUAN_TRI_SQL_SERVER.md) trình bày khung nhìn, thủ tục, vai trò, sao lưu và khôi phục.
- Dữ liệu kiểm thử nằm riêng trong `sql/05_Du_lieu_mau.sql` và `sql/du-lieu-mau`. Để chạy trực tiếp script 05, chọn một cơ sở dữ liệu trống đã tạo bảng có tên bắt đầu bằng `CongMotCua_KiemThu`. Để cài cả tài khoản và tệp qua ứng dụng, dùng một bản sao dự án và chạy `setup.ps1 -Database CongMotCua_KiemThu -NapDuLieuMau`. Mật khẩu kiểm thử nằm trong `Identity.SamplePassword` của cấu hình cục bộ.
- [Danh mục khởi đầu](sql/06_Danh_muc_khoi_dau.sql) thêm hai bộ phận và ba thủ tục thực hành. Chọn cơ sở dữ liệu đã khởi tạo trong SSMS rồi chạy tập lệnh.

## Tài liệu kỹ thuật

- [Tài liệu API](docs/API.md)
- [Kết quả kiểm tra](docs/KET_QUA_KIEM_TRA.md)
- [Kịch bản kiểm thử](docs/KICH_BAN_KIEM_THU.md)
- [Tài liệu, sơ đồ và thiết kế](../README.md)
