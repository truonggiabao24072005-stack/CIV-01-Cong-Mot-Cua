# Kiểm thử và giới hạn xác nhận

Các kịch bản kiểm thử nằm ở Chương VII và trong thư mục [bảng biểu của báo cáo](../../02_Bang_bieu/bang-bieu/). Kết quả thực tế trên cơ sở dữ liệu kiểm tra riêng nằm tại [Kết quả kiểm tra](KET_QUA_KIEM_TRA.md).

## Phạm vi kiểm tra đã mô tả

| Nhóm | Nội dung |
|---|---|
| API nghiệp vụ | Đăng nhập/phân quyền, quyền chủ hồ sơ, tài liệu bắt buộc, đánh giá, tiếp nhận, phân công, bổ sung nhiều lần, từ chối, lập/phát hành/tải kết quả |
| Backend khi lỗi/tranh chấp | Hai lệnh sửa song song cùng version; một thành công, một 409; lịch sử/phiên bản chỉ tăng một lần. Nộp thiếu tệp rollback hồ sơ mới; thao tác thất bại không thêm lịch sử |
| Quyền cán bộ | Người chưa được giao không đọc; người bàn giao mất quyền đọc/tải; cán bộ cuối được đọc kết quả mình lập |
| DTO và danh mục | Null, chuỗi quá dài, mã sai; không thay chủ/trạng thái bằng trường lạ; danh mục dùng rồi giữ ID; hướng dẫn cập nhật được; nộp đủ 30 thành phần |
| Trạng thái tài khoản | Tài khoản ngừng hoạt động bị chặn cả token cũ/lần đăng nhập mới; không tự khóa tài khoản, khóa cán bộ còn việc hoặc ngừng bộ phận còn phụ thuộc |
| SQL Server | 13 bảng, 25 FK, 18 CHECK, 4 trigger, 2 view, 2 procedure và 2 role; chặn sửa/xóa lịch sử, sai thủ tục, phân công hiện hành trùng, trạng thái/số liệu sai |
| Lịch SLA | Ngày lịch/ngày làm việc, cuối tuần, ngày nghỉ, giờ Việt Nam, snapshot, tính lại/giữ nguyên/bù thời gian, giữ hạn gốc, chưa tiếp nhận chưa có SLA |
| Giao diện | Đăng nhập và sửa nháp; tìm hồ sơ; tài liệu/lịch sử; quản lý thủ tục; báo cáo CSV; mobile; biểu mẫu 4 bước |
| Liên thông 3 vai trò | Nộp → đánh giá/tiếp nhận → phân công → ghi kết quả → phát hành → tải tệp, thực hiện bằng nút/biểu mẫu trên trình duyệt |

Kết quả đã thực hiện được ghi ở [KET_QUA_KIEM_TRA.md](KET_QUA_KIEM_TRA.md). Theo yêu cầu dừng kiểm thử và dọn file tạm, bản bàn giao không kèm công cụ, dữ liệu và mã test dựng thử.

## Giới hạn

Phạm vi kiểm tra gồm các chức năng xử lý hồ sơ và xác thực cục bộ. VNeID, chữ ký số, thanh toán và thông báo qua email/SMS chưa được tích hợp. Kiểm thử tải và đánh giá an ninh cần thực hiện trước khi triển khai rộng. Đơn vị vận hành cấu hình ngày nghỉ và thời hạn áp dụng.
