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
| [02_Co_so_du_lieu_SQL](../02_Co_so_du_lieu_SQL/) | Tệp SQL dùng để tạo 13 bảng, đối tượng quản trị và danh mục khởi đầu |
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

1. Đăng nhập với tài khoản tiếp nhận vừa cấp. Trong **Quản lý danh mục**, tạo hoặc cập nhật bộ phận chuyên môn, thủ tục, thành phần hồ sơ và thời hạn.
2. Dừng web. Cấp tài khoản chuyên môn bằng `cap-tai-khoan.ps1` với `-VaiTro GIAI_QUYET -MaBoPhan <mã-bộ-phận>` rồi khởi động lại. Mỗi cán bộ có mật khẩu riêng.
3. Người dân đăng ký tài khoản trên web. Giữ `appsettings.Local.json` và thư mục `.data` trên máy phục vụ để duy trì tài khoản và kho tệp.

Khi cập nhật bản chạy, dừng web rồi chạy `setup.ps1 -Build`. Nếu đã có cấu hình cục bộ, lệnh giữ nguyên cơ sở dữ liệu, kho tệp và kho tài khoản. Chỉ truyền `-Database` hoặc `-Server` khi chủ động cấu hình lại môi trường.

### SQL Server

- `setup.ps1` tự tạo database nếu cần và cài đối tượng quản trị từ tệp SQL bên ngoài thư mục mã nguồn. Khi giải nén, để `01_Ma_nguon` và `02_Co_so_du_lieu_SQL` cùng cấp.
- Để cài thủ công, mở [CongMotCua_HoanChinh.sql](../02_Co_so_du_lieu_SQL/CongMotCua_HoanChinh.sql) trong SSMS và nhấn **Execute**. Tệp tạo database `CongMotCua_VanHanh` với 13 bảng và ba thủ tục khởi đầu nếu chưa có dữ liệu. Các mẫu truy vấn và sao lưu ở cuối tệp dùng khi cần.
- Để nạp tài khoản và tệp thử vào một database riêng, dùng bản sao dự án và chạy `setup.ps1 -Database CongMotCua_KiemThu -NapDuLieuMau`. Mật khẩu thử nằm trong `Identity.SamplePassword` của cấu hình cục bộ.
