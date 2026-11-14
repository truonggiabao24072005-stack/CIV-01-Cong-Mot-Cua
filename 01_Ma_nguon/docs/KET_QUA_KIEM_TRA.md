# Kết quả kiểm tra sản phẩm

Ứng dụng chạy trên .NET 9 và SQL Server với cơ sở dữ liệu, tài khoản và kho tệp kiểm tra riêng.

| Phạm vi | Kết quả |
|---|---|
| API và luồng ba vai trò | 109/109 bước đạt |
| API đợt rà soát cuối | 42/42 bước đạt |
| Kiểm tra lại hai lỗi đã xác nhận | 8/8 bước đạt |
| SQL Server | 13 bảng, 25 khóa ngoại, 18 CHECK, 4 trigger, 2 view, 2 thủ tục và 2 vai trò SQL |
| Trình duyệt | Mở danh mục, xem giấy tờ, đăng nhập người dân, thử điều kiện thiếu tệp và lưu, mở lại bản nháp |

TC19 xác nhận API chặn gửi bổ sung khi thiếu tệp được yêu cầu. Tệp mất dữ liệu hoặc sai kích thước bị từ chối. Thông báo kết quả và từ chối có căn cứ, kết luận và hướng dẫn. Cán bộ cũ mất quyền sau bàn giao. Hai thao tác cùng phiên bản chỉ cho một yêu cầu cập nhật thành công.

Kiểm tra trực tiếp trên SQL Server xác nhận trigger chặn sửa nhật ký (51005) và chặn gắn tệp vào thành phần của thủ tục khác (51001). Chỉ mục duy nhất chặn phân công hiệu lực trùng (2601). Dữ liệu sau khi duyệt bổ sung có đúng bản hiện hành và giữ hạn gốc. Hồ sơ đã trả có mốc kết thúc trùng thời điểm phát hành. Tập lệnh SQL tổng hợp tạo lược đồ có tên cột, kiểu dữ liệu và tính cho phép NULL khớp với lược đồ do ứng dụng tạo.

[Chi tiết từng bước kiểm tra API](kiem-thu/Ket_qua_API.csv).

Đợt rà soát cuối đã kiểm tra tệp bản nháp bị mất, việc hiển thị tệp đã lưu, cập nhật danh mục, ngừng tiếp nhận thủ tục và bảo toàn hồ sơ đang xử lý, kiểm soát vai trò, tiếp nhận và phát hành kết quả. Lệnh cài lại giữ cấu hình cơ sở dữ liệu và kho tệp hiện có. [Chi tiết 42 lượt kiểm tra](kiem-thu/Ket_qua_ra_soat_cuoi.csv).
