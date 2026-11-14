# Kịch bản kiểm thử nghiệp vụ

Chạy trên bản sao dự án với `setup.ps1 -Database CongMotCua_KiemThu -NapDuLieuMau` và cơ sở dữ liệu trống. Các mã và tài khoản dưới đây nằm trong bộ dữ liệu kiểm thử.

## Luồng chính khoảng 8–10 phút

1. Mở trang chủ, tìm một thủ tục, xem thành phần. Nêu cách tham khảo biểu mẫu dịch vụ công và phạm vi cơ quan giả định.
2. Bấm Nộp trực tuyến, đăng nhập `minhanh`. Thủ tục vừa chọn được giữ nguyên. Giới thiệu 4 bước kê khai; nhập nội dung, đính kèm hai TXT/PDF mẫu, xác nhận và nộp.
3. Ghi lại mã gửi. Mở chi tiết để chỉ ra chưa có mã chính thức/hạn trả. Đăng xuất.
4. Đăng nhập `tiepnhan`, tìm đúng mã gửi. Trong Tài liệu và đánh giá, đánh giá Đạt từng bản. Chọn Xác nhận tiếp nhận. Trình bày mã hồ sơ, hạn trả và nút In biên nhận.
5. Luân chuyển hồ sơ cho `giaiquyet`, ghi nội dung bàn giao. Đăng xuất.
6. Đăng nhập `giaiquyet`, mở hồ sơ được giao. Nhập căn cứ, kết luận, kết quả cụ thể và hướng dẫn, rồi tải tệp kết quả. Chỉ ra trạng thái Đã có kết quả nhưng chưa phát hành. Đăng xuất.
7. Đăng nhập `tiepnhan`, mở hồ sơ và Xác nhận trả kết quả. Giới thiệu thời điểm kết thúc SLA, báo cáo và xuất CSV.
8. Đăng nhập lại `minhanh`, mở hồ sơ, xem lịch sử và tải kết quả. Đăng xuất để tra cứu công khai bằng mã, nhấn mạnh trang công khai không trả tệp/thông tin cá nhân.

Tệp TXT mẫu có thể tạo bằng Notepad với vài dòng “Tài liệu mô phỏng phục vụ đồ án”. Không đổi đuôi TXT thành PDF; hệ thống kiểm tra định dạng thực.

## Nhánh bổ sung

Tại hồ sơ đang kiểm tra hoặc đang được giao, tạo yêu cầu gồm vấn đề, lý do, hướng dẫn và thành phần liên quan. Người dân gửi đáp ứng lần 1. Cán bộ đánh giá Chưa đạt và ghi lý do; người dân gửi lần 2. Cán bộ đánh giá Đạt từng tệp của lần 2, rồi chấp nhận cả lần bổ sung. Mở lịch sử để đối chiếu: các lần gửi và bản tệp cũ vẫn còn, chỉ bản được chấp nhận thành hiện hành.

## Dữ liệu sẵn có

| Hồ sơ | Tình trạng | Dùng để trình diễn |
|---|---|---|
| demo-hs-1 | Nháp của minhanh | Sửa nội dung, đính kèm, nộp |
| GUI-DEMO-002 | Chờ kiểm tra | Đánh giá và tiếp nhận |
| HS-DEMO-003 | Đã tiếp nhận | Phân công |
| HS-DEMO-004 | Đang giải quyết | giaiquyet ghi kết quả/yêu cầu bổ sung |
| HS-DEMO-005 | Đã có kết quả | tiepnhan phát hành |
| HS-DEMO-006 | Đã trả; thuộc hoangnam | Đăng nhập hoangnam để tải, kiểm tra tách quyền |

Để lặp lại từ đầu, dùng tên cơ sở dữ liệu kiểm thử mới và tùy chọn `-NapDuLieuMau`.
