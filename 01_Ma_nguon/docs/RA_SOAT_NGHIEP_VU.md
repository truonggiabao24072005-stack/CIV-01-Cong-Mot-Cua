# Rà soát nghiệp vụ Cổng một cửa

Mô hình giữ UC01–UC09 và ba vai trò đăng nhập. UC06A/UC06B tách hai lượt trong UC06. UC09A là lối tra cứu dành cho khách, không phát sinh vai trò đăng nhập hoặc quyền xem chi tiết. Phụ lục UC04 dùng cùng mã với sơ đồ tổng quát.

## Cách đọc quan hệ

- `A «include» B` nghĩa là luồng của A cần thực hiện hành vi B. Mũi tên nét đứt đi từ A đến B.
- `X «extend» A` nghĩa là X được chèn tại một điểm của A khi đúng điều kiện. Mũi tên nét đứt đi từ X về A. Điều kiện được ghi dưới từng sơ đồ.
- Đăng nhập và quyền truy cập là điều kiện vào các màn hình có phiên. Chỉ UC01 mô tả thao tác đăng nhập. Thứ tự nộp, tiếp nhận, xử lý và trả kết quả đọc qua sơ đồ trạng thái hoặc luồng bên dưới. [OMG UML 2.5.1, chương 18](https://www.omg.org/spec/UML/2.5.1/PDF)

## Luồng và kết quả theo từng use case

| Use case | Bước bắt buộc và kiểm tra | Nhánh hoặc điều kiện kết thúc |
|---|---|---|
| UC01 Đăng nhập | Xác thực và phân quyền theo tài khoản đang hoạt động. | Sai thông tin thì báo lỗi tại biểu mẫu. Khi phiên hết hạn, đăng nhập lại để tiếp tục. |
| UC02 Tạo và nộp hồ sơ | Kê khai theo thủ tục. Nhánh **Nộp hồ sơ** luôn kiểm tra nội dung và tệp bắt buộc rồi mới cấp mã gửi. | **Lưu bản nháp** khi người dân chọn làm tiếp sau. Bản nháp có thể sửa nội dung, liên hệ và thay tệp trước khi nộp. |
| UC03 Kiểm tra và tiếp nhận | Đối chiếu thành phần và trạng thái đánh giá. Chỉ tiếp nhận khi có đủ tệp bắt buộc, mọi bản hiện hành đều có đánh giá Đạt. | **Tiếp nhận** thì cấp mã hồ sơ, biên nhận và hạn trả. **Yêu cầu bổ sung** nếu có thể khắc phục. **Từ chối tiếp nhận** nếu có căn cứ, ghi lý do và công bố. |
| UC04 Luân chuyển và phân công | Hồ sơ đã tiếp nhận hoặc đang giải quyết, không có yêu cầu bổ sung mở. Chọn cán bộ đang hoạt động đúng bộ phận và ghi nội dung bàn giao. | Nếu có phân công cũ đang mở, kết thúc lượt cũ trong cùng giao dịch với việc mở lượt mới. |
| UC05 Yêu cầu bổ sung | Ghi từng mục cần sửa, lý do, hướng dẫn và bước sẽ quay lại. Mỗi hồ sơ chỉ có một yêu cầu đang mở. | Cán bộ tiếp nhận thao tác ở giai đoạn kiểm tra ban đầu. Cán bộ chuyên môn thao tác trên hồ sơ đang được giao. |
| UC06A Gửi đáp ứng bổ sung | Hệ thống kiểm tra tệp cho từng thành phần cần nộp lại, nội dung đề nghị thay thế và giải trình theo mục tương ứng rồi lưu lần gửi. | Nội dung đề nghị mới thay toàn bộ nội dung cũ sau khi cán bộ chấp nhận. |
| UC06B Kiểm tra lần gửi bổ sung | Cán bộ phụ trách đối chiếu lần gửi với yêu cầu. Trước khi chấp nhận, từng tệp mới nếu có phải được đánh giá Đạt. | **Đạt** thì dùng bản tệp mới, áp dụng điều chỉnh và quay lại đúng bước trước. **Chưa đạt** thì ghi lý do, giữ bản cũ và mở lại lượt gửi cho người nộp. Hạn xử lý sau bổ sung theo chính sách đã lưu khi tiếp nhận. |
| UC07 Xem xét và ghi nhận kết luận | Kiểm tra người đang được giao, trạng thái xử lý và yêu cầu bổ sung mở. | **Thành công** cần kết luận và tệp kết quả, sau đó chờ phát hành. **Cần thêm thông tin** thì mở UC05 trước khi kết luận. **Từ chối giải quyết** phải ghi căn cứ và lý do. |
| UC08 Trả kết quả điện tử | Kiểm tra kết quả thành công và tệp sẵn sàng. Khi phát hành, ghi người phát hành và thời điểm kết thúc xử lý. | Người nộp xem được kết quả thành công sau phát hành. Kết quả từ chối đã công bố tại thao tác từ chối. |
| UC09 Theo dõi hồ sơ và nhận kết quả | Kiểm tra quyền xem hồ sơ và tệp theo vai trò. | In biên nhận sau tiếp nhận. Tải kết quả khi đã phát hành và có tệp. |
| UC09A Tra cứu công khai | Xác minh thử thách và mã tra cứu. | Hiển thị tiến độ được công bố. Nội dung, tài liệu và kết quả chi tiết nằm trong màn hình có phân quyền. |

## Kịch bản dùng thử

1. **Luồng thuận.** Người dân chọn thủ tục và nộp. Cán bộ tiếp nhận đánh giá từng tệp, tiếp nhận và phân công. Cán bộ chuyên môn lập kết quả. Cán bộ tiếp nhận phát hành, sau đó người dân tải kết quả.
2. **Luồng bổ sung.** Cán bộ ghi mục cần sửa và hướng dẫn. Người dân gửi lần 1. Cán bộ trả lại kèm lý do. Người dân xem lý do và gửi lại. Khi đạt, hồ sơ quay về giai đoạn đã mở yêu cầu, giữ lịch sử các lần gửi.
3. **Luồng từ chối.** Thử riêng từ chối tiếp nhận và từ chối giải quyết. Trường hợp trước tiếp nhận chỉ có mã gửi và chưa có hạn giải quyết. Trường hợp đã tiếp nhận kết thúc thời hạn ở thời điểm từ chối.
4. **Xung đột thao tác.** Hai cán bộ mở cùng hồ sơ, một người cập nhật trước. Yêu cầu dựa trên bản cũ bị từ chối. Tải lại hồ sơ trước khi chọn thao tác mới.

## Thao tác trên giao diện

- Bàn làm việc hiển thị các nhóm hồ sơ theo vai trò. Nhóm bổ sung chỉ tính hồ sơ đang đến lượt người đăng nhập. Người dân thấy yêu cầu đang chờ gửi. Cán bộ thấy lần gửi đang chờ mình kiểm tra.
- Danh sách và chi tiết đều ghi bước tiếp theo. Mở chi tiết từ danh sách rồi bấm quay về sẽ khôi phục bộ lọc và số trang.
- Khi gửi bổ sung, nội dung cần sửa và hướng dẫn nằm ngay trong biểu mẫu. Khi đánh giá, cán bộ chọn rõ Đạt hoặc Chưa đạt. Lựa chọn Đạt cho cả lần gửi chỉ mở khi từng tệp mới đã đạt. Hồ sơ có kết quả được công bố có nút mở nhanh phần kết quả.

## Điều kiện chuyển từ đồ án sang vận hành tại đơn vị

| Nhóm việc | Đầu việc trước khi nhận hồ sơ thật |
|---|---|
| Quy trình | Đơn vị duyệt danh mục thủ tục, thành phần, lịch làm việc, thời hạn, chính sách bổ sung và thẩm quyền phát hành hoặc từ chối. Chốt luồng rút hồ sơ và xử lý yêu cầu quá hạn theo quy trình của đơn vị. |
| Tài khoản | Tắt dữ liệu và tài khoản mẫu. Cấp, thu hồi quyền theo nhân sự. Chọn và nghiệm thu cơ chế xác thực của đơn vị. |
| Hồ sơ điện tử | Chốt mẫu phiếu, ký và phát hành kết quả, lưu trữ hồ sơ, quét tệp tải lên, quyền xem và nhật ký truy cập theo quy định nội bộ. |
| Vận hành | Triển khai HTTPS, cấu hình quản lý khóa, sao lưu đồng bộ SQL Server và kho tệp, thử khôi phục, giám sát lỗi và quá hạn. |
| Nghiệm thu | Chạy thử bằng dữ liệu phi cá nhân với cán bộ từng bộ phận. Xác nhận các nhánh bổ sung, từ chối, bàn giao, xung đột và sự cố mất kết nối trước khi đưa vào sử dụng. |
