# Bàn giao đồ án

Mở dự án bằng VS Code và làm theo [hướng dẫn cài đặt](../README.md). Bản cài mới tạo cơ sở dữ liệu trống. Quản trị viên cấp tài khoản cán bộ, sau đó cán bộ tiếp nhận cấu hình danh mục thủ tục. Có thể kiểm tra quy trình trên cơ sở dữ liệu riêng theo [kịch bản kiểm thử](KICH_BAN_KIEM_THU.md).

- [`wwwroot`](../wwwroot/) chứa các trang HTML, CSS và JavaScript.
- [`Api`](../Api/) chứa controller, mô hình và kết nối SQL Server. [`Services`](../Services/) chứa các lớp xử lý nghiệp vụ và xác thực.
- [Quản trị SQL Server](QUAN_TRI_SQL_SERVER.md) mô tả quyền, view, truy vấn và sao lưu.
- [Đối chiếu thiết kế](DOI_CHIEU_THIET_KE.md) liên hệ use case với chức năng và dữ liệu.

Khi chuyển máy, sao lưu và khôi phục đồng bộ cơ sở dữ liệu SQL, kho tệp và kho danh tính. Tạo lại cấu hình theo đường dẫn trên máy mới. Bản chạy được tạo trên máy bằng script cài đặt.
