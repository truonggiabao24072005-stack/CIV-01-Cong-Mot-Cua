# CIV-01 — Cổng một cửa theo dõi hồ sơ dịch vụ công

Đồ án mô phỏng quy trình giải quyết hồ sơ dịch vụ công, từ lúc người dân nộp hồ sơ đến khi nhận kết quả điện tử.

**Sinh viên thực hiện** · Trương Gia Bảo — 1250080020

## Chức năng chính

- **Người dân** tra cứu thủ tục, xem danh sách giấy tờ cần chuẩn bị, nộp và bổ sung hồ sơ, theo dõi tiến độ và nhận kết quả.
- **Cán bộ tiếp nhận** kiểm tra hồ sơ, tiếp nhận hoặc từ chối, phân công xử lý và phát hành kết quả điện tử.
- **Cán bộ chuyên môn** kiểm tra tài liệu, yêu cầu bổ sung khi cần và ghi nhận kết quả xử lý.

Ứng dụng sử dụng ASP.NET Core và Entity Framework Core trên .NET 9, cơ sở dữ liệu SQL Server và giao diện HTML, CSS, JavaScript.

## Chạy bằng VS Code

Cài **.NET SDK 9** và khởi động **SQL Server**. Tài khoản Windows dùng để khởi tạo cần có quyền tạo cơ sở dữ liệu.

1. Tải cả kho bằng **Code → Download ZIP** rồi giải nén. Mở thư mục đã giải nén trong VS Code.
2. Mở Terminal PowerShell và chạy các lệnh sau. Chạy lệnh `setup.ps1` trong lần cài đầu tiên.

   ```powershell
   cd .\01_Ma_nguon
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup.ps1
   dotnet watch run
   ```

3. Mở [http://localhost:5080](http://localhost:5080) trong trình duyệt.

Xem cách chọn máy chủ SQL Server, cấp tài khoản theo vai trò hoặc nạp dữ liệu thử trong [hướng dẫn chạy chi tiết](01_Ma_nguon/README.md#cài-đặt-và-chạy-bằng-vs-code).

## Mã nguồn và tài liệu thiết kế

| Thư mục | Nội dung |
| --- | --- |
| [01_Ma_nguon](01_Ma_nguon/) | Mã ứng dụng và hướng dẫn cài đặt |
| [02_Co_so_du_lieu_SQL](02_Co_so_du_lieu_SQL/) | [Tệp SQL khởi tạo](02_Co_so_du_lieu_SQL/CongMotCua_HoanChinh.sql) gồm 13 bảng, danh mục ban đầu và các đối tượng quản trị |
| [03_So_do_Drawio](03_So_do_Drawio/) | Sơ đồ gốc `.drawio` và [cách mở bằng Draw.io](03_So_do_Drawio/README.md) |
| [04_Giao_dien_Figma](04_Giao_dien_Figma/) | Bản thiết kế gốc `.fig` và [cách nhập vào Figma](04_Giao_dien_Figma/README.md) |
