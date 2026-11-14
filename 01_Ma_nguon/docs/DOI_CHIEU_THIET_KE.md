# Đối chiếu sản phẩm với thiết kế

## 3 tác nhân và 9 use case

| Use case | Thao tác trong sản phẩm | Vai trò |
|---|---|---|
| UC01 Đăng nhập | Biểu mẫu đăng nhập, nhận danh tính và phân quyền | Cả 3 vai trò |
| UC02 Tạo và nộp hồ sơ | Biểu mẫu 4 bước; lưu/sửa nháp; cấp mã gửi khi nộp | Người dân |
| UC03 Kiểm tra và tiếp nhận | Đánh giá bản tệp; đủ điều kiện mới cấp mã chính thức, biên nhận và hạn trả | Tiếp nhận |
| UC04 Luân chuyển và phân công | Chọn cán bộ đúng bộ phận; đóng phân công cũ, thêm phân công mới | Tiếp nhận |
| UC05 Yêu cầu bổ sung | Liệt kê thành phần/trường cần sửa, vấn đề, lý do, hướng dẫn | Tiếp nhận hoặc cán bộ đang được giao |
| UC06 Gửi và kiểm tra bổ sung | Lưu từng lần gửi; kiểm tra từng tệp và kết luận lần gửi | Người dân và cán bộ phụ trách |
| UC07 Ghi nhận kết quả xử lý | Kết luận, tệp kết quả; hoặc từ chối có căn cứ | Cán bộ chuyên môn |
| UC08 Trả kết quả điện tử | Phát hành kết quả thành công; chốt thời điểm kết thúc SLA | Tiếp nhận |
| UC09 Theo dõi và nhận kết quả | Danh sách, chi tiết, lịch sử, tải kết quả đã phát hành; tra cứu công khai giới hạn | Theo quyền hồ sơ |

Quản lý danh mục và thống kê là màn hình hỗ trợ của vai trò tiếp nhận, không bổ sung tác nhân thứ tư. Không có quyền sửa tùy ý trạng thái hồ sơ trong giao diện quản trị.

## 13 bảng và cách chia liên kết

| Bảng | Chức năng và bảng cha |
|---|---|
| BoPhan | Danh mục bộ phận; cha của người dùng, thủ tục và phân công |
| NguoiDung | Danh tính nghiệp vụ; khóa Issuer + Subject duy nhất; BoPhanId tùy chọn |
| ThuTuc | Danh mục thủ tục và chính sách mẫu; thuộc bộ phận chuyên môn |
| ThanhPhanYeuCau | Các thành phần của một thủ tục; FK ThuTucId |
| HoSo | Hồ sơ của người nộp theo thủ tục; lưu chính sách SLA chụp tại tiếp nhận |
| PhanCongXuLy | Các lượt bàn giao của hồ sơ; FK hồ sơ, người chuyển, cán bộ và bộ phận nhận |
| TaiLieuHoSo | Tệp theo hồ sơ và thành phần; FK LanId chỉ có khi là tệp bổ sung |
| KiemTraThanhPhan | Các lần đánh giá một bản tài liệu; FK TaiLieuId và CanBoId |
| YeuCauBoSung | Yêu cầu bổ sung của hồ sơ; trạng thái và giai đoạn quay lại |
| NoiDungBoSung | Các mục cần sửa thuộc một yêu cầu; thành phần có thể để trống khi sửa thông tin |
| LanBoSung | Các lần người dân đáp ứng một yêu cầu; không lặp FK HoSoId |
| KetQuaHoSo | Tối đa một kết quả nghiệp vụ cho hồ sơ; tách lập và phát hành |
| LichSuXuLy | Sự kiện xử lý chỉ thêm; gắn hồ sơ và người thực hiện |

KiemTraThanhPhan truy ra hồ sơ qua TaiLieuHoSo; LanBoSung truy ra hồ sơ qua YeuCauBoSung; NoiDungBoSung truy ra hồ sơ qua YeuCauBoSung. Không thêm HoSoId vào các bảng này chỉ để nối trực tiếp với hồ sơ.

Các khóa ngoại của người thao tác được lưu trong SQL. Kho danh tính đăng nhập cục bộ được lưu riêng, bên ngoài 13 bảng nghiệp vụ.

## Những quy tắc cần giữ khi bảo vệ

1. Lưu nháp chưa có mã gửi. Nộp có mã gửi nhưng chưa có mã hồ sơ chính thức và SLA giải quyết.
2. Tiếp nhận cần đủ tệp bắt buộc và đánh giá Đạt mới nhất cho các bản hiện hành.
3. Chỉ một phân công hiện tại và một yêu cầu bổ sung đang mở cho mỗi hồ sơ.
4. Gửi bổ sung chưa đồng nghĩa đã đạt. Bản mới chờ đánh giá chưa thay bản hiện hành.
5. Chấp nhận lần bổ sung mới áp dụng thông tin sửa và chính sách tính lại/giữ nguyên/bù thời gian theo cấu hình đã chụp. Với TINH_LAI, mốc là thời điểm gửi của lần được chấp nhận.
6. HanTraGoc giữ nguyên. Thay cấu hình thủ tục không làm thay chính sách của hồ sơ đã tiếp nhận.
7. Lập kết quả thành công chưa kết thúc SLA. Phát hành mới đồng thời chốt PhatHanhLuc và NgayKetThuc.
8. Kết quả từ chối có căn cứ được công bố trong thao tác từ chối; từ chối trước tiếp nhận không phát sinh SLA giải quyết.
9. Người dân chỉ xem hồ sơ của mình; chuyên môn xem hồ sơ đang được giao và hồ sơ kết thúc mà mình là cán bộ xử lý cuối. Tệp luôn tải qua API có kiểm tra quyền.
10. Mỗi cập nhật dùng RowVersion, khóa hàng và một giao dịch chứa cả nghiệp vụ/lịch sử. Xung đột trả 409 để tải lại.

SLA trong đồ án dùng ngày lịch/ngày làm việc; cuối tuần và danh sách ngày nghỉ chụp từ cấu hình. Thời hạn chỉ cập nhật sau khi lần bổ sung được chấp nhận. Phần phản hồi ban đầu được lưu trong cấu hình thủ tục; không triển khai thông báo email/SMS hoặc lịch làm việc theo giờ của cơ quan thật.

Xem [rà soát nghiệp vụ](RA_SOAT_NGHIEP_VU.md) và [các sơ đồ phân rã Use Case](../../03_So_do_Drawio/02_Use_Case/) để đối chiếu các bước bắt buộc, nhánh có điều kiện và điều kiện đưa hệ thống vào vận hành.
