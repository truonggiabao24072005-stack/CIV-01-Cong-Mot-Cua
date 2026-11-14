# Quản trị SQL Server bằng SSMS

Kết nối Database Engine bằng Windows Authentication. Máy chủ mặc định là `localhost` và cơ sở dữ liệu vận hành mặc định là `CongMotCua_VanHanh`.

## Cài đặt schema và dữ liệu

Cách khuyến nghị: chạy `scripts/setup.ps1`. Ứng dụng tạo 13 bảng và dữ liệu mẫu; script tiếp tục cài views, procedures, triggers và roles. Dùng Object Explorer trong SSMS để xem Tables/Keys/Indexes/Views/Programmability/Security.

Cách chạy SQL thủ công trên database mới: `00_Tao_database.sql` → `01_Tao_13_bang.sql` → `02_Quan_tri_va_rang_buoc.sql` → `05_Du_lieu_mau.sql` nếu cần dữ liệu mẫu. Script 01 chỉ chạy trên database trống. Không chạy thêm 05 nếu ứng dụng đã tạo dữ liệu. Đổi tên database thống nhất trong các script khi dùng tên khác.

Script 05 và thư mục `sql/du-lieu-mau/files` dùng riêng để đối chiếu dữ liệu kiểm thử SQL. Để có đủ tài khoản và tệp trên bản kiểm thử, dùng tùy chọn `-NapDuLieuMau` của `setup.ps1` trong một bản sao dự án và chỉ định tên cơ sở dữ liệu kiểm thử trống.

## Đối tượng quản trị

| Đối tượng | Cách dùng |
|---|---|
| 03_Truy_van_quan_tri.sql | Liệt kê bảng, FK, chỉ mục, dòng dữ liệu và phát hiện trùng quan hệ hiện hành |
| v_HoSoTheoDoi | Theo dõi hồ sơ và thông tin liên quan |
| v_PhanCongHienTai | Chỉ các lượt phân công chưa kết thúc |
| sp_ThongKeThuTuc | Thống kê theo khoảng ngày nộp; tham số @TuNgay, @DenNgay |
| sp_LichSuHoSo | Nhật ký theo @HoSoId |
| cmc_bao_cao | SELECT các view và EXECUTE các procedure báo cáo |
| cmc_ung_dung | SELECT/INSERT/UPDATE nghiệp vụ, DELETE thành phần khi sửa thủ tục chưa sử dụng; không sửa/xóa lịch sử |

Database role khác vai trò người dùng trong ứng dụng: một role SQL kiểm soát kết nối database; ba vai trò nghiệp vụ được API kiểm tra theo người dùng và hồ sơ. Đổi role SQL không tự cấp quyền nghiệp vụ.

Mẫu gán tài khoản Windows vào role báo cáo, cần thay tên thực và quyền quản trị SQL:

```sql
-- USE [master];
-- CREATE LOGIN [TENMAY\TaiKhoanBaoCao] FROM WINDOWS;
-- USE [CongMotCua_VanHanh];
-- CREATE USER [TENMAY\TaiKhoanBaoCao] FOR LOGIN [TENMAY\TaiKhoanBaoCao];
-- ALTER ROLE [cmc_bao_cao] ADD MEMBER [TENMAY\TaiKhoanBaoCao];
```

## Ràng buộc quan trọng

- Filtered unique index: một phân công còn hiệu lực, một yêu cầu bổ sung mở, một tài liệu hiện hành theo thành phần/hồ sơ; unique kết quả theo hồ sơ.
- Trigger kiểm tra tài liệu/thành phần/lần bổ sung cùng hồ sơ và thủ tục; cán bộ nhận đúng bộ phận; cấm UPDATE/DELETE lịch sử.
- EF Core đã tắt SQL OUTPUT trực tiếp ở các bảng có trigger để các lệnh ghi hoạt động đúng trên SQL Server.
- 18 CHECK kiểm soát miền trạng thái, vai trò, SLA, phiên bản, dung lượng và thứ tự thời gian.
- Trigger và FK giữ toàn vẹn cấu trúc. Chuyển trạng thái nghiệp vụ phải đi qua API có giao dịch và kiểm tra quyền, không UPDATE trực tiếp HoSo để bỏ qua quy trình.

## Sao lưu và khôi phục

`scripts/backup.ps1` tạo backup COPY_ONLY, CHECKSUM và chạy RESTORE VERIFYONLY, đồng thời chép tệp/tài khoản/cấu hình vào thư mục backup trong `.data/backups`. Đường dẫn SQL backup là đường dẫn trên máy SQL Server; tài khoản dịch vụ SQL phải có quyền ghi, tài khoản Windows chạy script cần quyền đọc để chép về. Nếu thư mục backup SQL không cho tài khoản Windows đọc, script ghi rõ vị trí .bak đã verify trên máy chủ và vẫn sao lưu tệp/identity/cấu hình; DBA cần chép thêm .bak, kiểm tra localDatabaseCopy trong backup-info.json trước khi di chuyển.

`sql/04_Sao_luu_khoi_phuc.sql` là mẫu lệnh SSMS. Khi khôi phục, dùng tên database mới và MOVE đúng logical filename theo FILELISTONLY. Không tự động ghi đè database đang dùng. Khôi phục cả `.data/files` và `.data/identity`, cập nhật ConnectionStrings trong appsettings.Local.json, rồi chạy lại ứng dụng. Một file .bak không chứa nội dung PDF/ảnh/TXT của hồ sơ.

Nếu di chuyển thư mục dự án, chạy lại setup để cập nhật đường dẫn tệp và danh tính. Không gửi thư mục identity có khóa ký/mật khẩu băm lên kho công khai. Bản ZIP bàn giao không chứa khóa ký của máy phát triển.
