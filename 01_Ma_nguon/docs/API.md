# API Cổng một cửa

Địa chỉ bản chạy: `http://localhost:5080/api`. Swagger tương tác, có sẵn tài nguyên khi chạy offline: `http://localhost:5080/swagger`. JSON dùng camelCase; thời gian trả về UTC có Z, giao diện hiển thị theo giờ Việt Nam.

## Xác thực

```http
POST /api/auth/login
Content-Type: application/json

{"username":"minhanh","password":"<mat-khau-cuc-bo>"}
```

Nhận `{ "accessToken": "...", "expiresIn": 3600 }`. Các API riêng cần header `Authorization: Bearer <accessToken>`. Trong Swagger, dán token vào Authorize. Vai trò được đối chiếu lại với SQL; khóa tài khoản làm token còn hạn mất quyền nghiệp vụ.

Đăng ký chỉ tạo vai trò người dân. Các vai trò cán bộ được cấp sẵn trong dữ liệu mẫu, không lấy vai trò từ body đăng ký.

## Các nhóm API

| Phương thức và đường dẫn | Nội dung/quyền |
|---|---|
| GET /health; GET /config | Trạng thái SQL và cấu hình công khai |
| POST /auth/login; POST /auth/register | Đăng nhập, đăng ký người dân |
| GET /me; PUT /me | Danh tính nghiệp vụ, cập nhật số điện thoại của mình |
| GET /thu-tuc | Thủ tục đang hoạt động; công khai |
| GET /thu-tuc/{id}/thanh-phan | Thành phần của thủ tục đang tiếp nhận. Thủ tục đã ngừng nhận trả 404 ở đường dẫn công khai. |
| GET /tra-cuu/thu-thach; POST /tra-cuu | Xác nhận dùng một lần, tra cứu mã giới hạn thông tin |
| GET /can-bo | Cán bộ chuyên môn đang hoạt động; không dành cho người dân |
| GET /ho-so | Lọc theo quyền với q, thuTucId, status, deadline=overdue, work=supplement, page, pageSize. Dùng sort=oldest hoặc sort=deadline để đổi thứ tự và includeFilters=true để nhận thủ tục trong phạm vi quyền xem. |
| GET /thong-ke | Thống kê theo phạm vi quyền, gồm boSungCanThaoTac cùng phạm vi với work=supplement |
| POST /ho-so | Tạo nháp hoặc tạo và nộp; người dân |
| GET /ho-so/{id} | Chi tiết, tệp, yêu cầu/lần bổ sung, phân công, lịch sử theo quyền |
| PUT /ho-so/{id}/nhap?version=n | Sửa nháp của mình; giữ thủ tục đã chọn |
| POST /ho-so/{id}/{thaoTac} | Các thao tác nghiệp vụ ở bảng dưới |
| POST /tep | Upload multipart/form-data, tên field file, tối đa 10 MB |
| GET /tai-lieu/{id} | Tải bản tài liệu sau kiểm tra quyền hồ sơ |
| GET /ho-so/{id}/tep-ket-qua | Tải kết quả; người dân chỉ tải sau phát hành |
| GET /quan-tri | Danh mục/bộ phận/người dùng; chỉ tiếp nhận |
| POST /quan-tri/bo-phan | Thêm hoặc sửa bộ phận. Từ chối ngừng hoạt động khi còn thủ tục đang dùng, hồ sơ chờ xử lý hoặc phân công hiệu lực. |
| POST /quan-tri/thu-tuc | Thêm/sửa thủ tục, thành phần, SLA; giữ cấu trúc khi đã dùng |
| POST /quan-tri/nguoi-dung | Bật/tắt hoạt động; không tự khóa hoặc khóa cán bộ còn việc |
| GET /bao-cao | Tiếp nhận xem thống kê theo thủ tục. Có thể lọc ngày nộp theo giờ Việt Nam bằng tuNgay, denNgay dạng yyyy-MM-dd. |

Swagger liệt kê các đường dẫn và phương thức của API. Khi dùng Keycloak, các đường dẫn đăng nhập và đăng ký cục bộ được ẩn.

## Nháp và nộp

Upload từng tệp trước, lấy `key` từ phản hồi; truyền key trong tài liệu. Không truyền đường dẫn ổ đĩa hoặc tự sửa chủ hồ sơ/trạng thái.

```json
{
  "thuTucId": "tt-1",
  "noiDung": "Đề nghị giải quyết nội dung hồ sơ mẫu.",
  "nop": true,
  "thongTinLienHe": { "soDienThoai": "0900000000", "diaChi": "Địa chỉ mẫu" },
  "taiLieu": [
    { "thanhPhanId": "tp-1-1", "tepKey": "key_nhan_tu_upload_1" },
    { "thanhPhanId": "tp-1-2", "tepKey": "key_nhan_tu_upload_2" }
  ]
}
```

`nop=false` cho phép lưu thiếu thành phần. `nop=true` cần nội dung và đủ bản hiện hành cho thành phần bắt buộc. Cả hai cách nộp đều kiểm tra dữ liệu tệp hiện hành trong kho. Thành công trả thông tin hồ sơ, mã gửi và version. Chưa tiếp nhận nên chưa có mã hồ sơ chính thức/ngày hẹn trả. Tối đa 30 bản tệp một lần gửi, mỗi thành phần một bản trong lần đó.

Khi lập thông báo qua `ket-qua` hoặc `tu-choi`, gửi kết luận, `canCuXuLy` và `huongDanThucHien`. Thao tác `ket-qua` cần thêm `ketQuaCuThe`. Backend ghép các phần thành thông báo tối đa 6000 ký tự.

## Thao tác và điều kiện

Mọi action cần `version` mới nhất từ GET chi tiết. Các trường còn lại chỉ dùng theo thao tác.

| action | Vai trò và điều kiện | Trường cần truyền |
|---|---|---|
| nop | Người dân, bản nháp của mình | version |
| kiem-tra-tai-lieu | Cán bộ phụ trách đúng giai đoạn | taiLieuId, dat; lyDo nếu chưa đạt |
| tiep-nhan | Tiếp nhận, DA_GUI, không bổ sung mở; tệp hiện hành đã đạt | version |
| luan-chuyen | Tiếp nhận, đã nhận/đang giải quyết | canBoNhanId, boPhanNhanId, noiDung bàn giao |
| yeu-cau-bo-sung | Tiếp nhận khi kiểm tra hoặc cán bộ đang được giao | noiDungBoSung: 1–20 mục vấn đề/lý do/hướng dẫn |
| gui-bo-sung | Chủ hồ sơ, yêu cầu đang chờ đáp ứng | `taiLieu` theo từng thành phần cần nộp lại, `thongTinDieuChinh` khi yêu cầu sửa nội dung, `noiDung` cho mục giải trình chung |
| kiem-tra-bo-sung | Cán bộ phụ trách, lần đáp ứng chờ kiểm tra | dat; lyDo nếu chưa đạt; bản tệp phải được đánh giá đạt trước khi chấp nhận |
| ket-qua | Chuyên môn đang được giao, không bổ sung mở | noiDung, canCuXuLy, huongDanThucHien, ketQuaCuThe, tepKey |
| tu-choi | Cán bộ phụ trách đúng giai đoạn, không bổ sung mở | noiDung, canCuXuLy, huongDanThucHien, lyDo |
| tra-ket-qua | Tiếp nhận, kết quả thành công đã sẵn sàng | version, noiDung tùy chọn |

Ví dụ yêu cầu bổ sung:

```json
{
  "version": 4,
  "noiDungBoSung": [{
    "thanhPhanId": "tp-1-1", "truongThongTin": null,
    "vanDe": "Bản đơn chưa rõ nội dung đề nghị",
    "lyDo": "Chưa đủ thông tin để kiểm tra",
    "huongDan": "Nộp lại bản thể hiện đầy đủ nội dung"
  }]
}
```

`thanhPhanId=null` khi yêu cầu sửa thông tin/giải trình chung. Gửi `thongTinDieuChinh` chỉ đề nghị sửa nội dung; backend áp dụng khi cán bộ chấp nhận lần bổ sung. Lập kết quả chưa công khai với người dân và chưa chốt SLA; thao tác phát hành mới hoàn tất.

## Mã lỗi và giao dịch

| HTTP | Ý nghĩa |
|---|---|
| 400 | DTO sai/thiếu, tài liệu hoặc điều kiện nghiệp vụ chưa đủ |
| 401 | Chưa đăng nhập, token sai hoặc hết hạn |
| 403 | Vai trò, tài khoản hoặc trách nhiệm xử lý không cho phép |
| 404 | Không có hồ sơ trong phạm vi quyền xem, tệp/đường dẫn không tồn tại |
| 409 | Phiên bản cũ, trạng thái xung đột, trùng ràng buộc hoặc tranh chấp SQL |
| 429 | Quá giới hạn truy cập; không tự gửi lặp liên tục |
| 500 | Lỗi chưa dự kiến; xem traceId để đối chiếu log backend |

Lỗi nghiệp vụ có dạng `{ "message": "...", "traceId": "..." }`. Đăng nhập không hợp lệ có thể nhận 401 từ tầng xác thực. Sau 409, đọc lại chi tiết và quyết định thao tác; không tự đổi version rồi gửi lại mù.

Mỗi lệnh hồ sơ khóa bản ghi, kiểm tra quyền/version, cập nhật dữ liệu và thêm lịch sử trong cùng giao dịch. Kiểm thử có trường hợp hai lệnh sửa song song, rollback khi nộp thiếu tệp, chống thay chủ hồ sơ bằng trường lạ và khóa quyền của tài khoản ngừng hoạt động.
