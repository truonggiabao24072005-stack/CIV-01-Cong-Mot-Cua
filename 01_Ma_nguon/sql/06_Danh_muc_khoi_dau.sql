-- Danh mục thực hành. Dùng trong cơ sở dữ liệu đã khởi tạo lược đồ Cổng một cửa.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;

DECLARE @BoPhan TABLE (Id varchar(50), Ma nvarchar(255), Ten nvarchar(255));
INSERT INTO @BoPhan VALUES
    ('khoi-dau-tiep-nhan', N'TH-TN', N'Bộ phận Tiếp nhận và Trả kết quả'),
    ('khoi-dau-chuyen-mon', N'TH-CM', N'Bộ phận chuyên môn');

INSERT dbo.BoPhan (BoPhanId, MaBoPhan, TenBoPhan, HoatDong)
SELECT Id, Ma, Ten, 1 FROM @BoPhan AS source
WHERE NOT EXISTS (SELECT 1 FROM dbo.BoPhan AS target WHERE target.MaBoPhan = source.Ma);

DECLARE @ChuyenMon varchar(50) = (
    SELECT BoPhanId FROM dbo.BoPhan WHERE MaBoPhan = N'TH-CM' AND HoatDong = 1
);
IF @ChuyenMon IS NULL
    THROW 51030, N'Bộ phận chuyên môn TH-CM đang ngừng hoạt động.', 1;

DECLARE @ThuTuc TABLE (Id varchar(50), Ma nvarchar(255), Ten nvarchar(255), MoTa nvarchar(max), SoNgay int, QuyTac nvarchar(255));
INSERT INTO @ThuTuc VALUES
    ('khoi-dau-ho-tich', N'TH-01', N'Cấp bản sao trích lục hộ tịch', N'Hồ sơ thực hành cho cá nhân tự nộp trực tuyến. Kê khai sự kiện hộ tịch, nơi đăng ký và số bản cần cấp. Giấy tờ đối chiếu chỉ nộp khi cần. Thời hạn hiển thị là thông số thực hành.', 3, N'TINH_LAI'),
    ('khoi-dau-chung-thuc', N'TH-02', N'Chứng thực bản sao từ bản chính', N'Hồ sơ thực hành cho bước kiểm tra giấy tờ. Cần xuất trình bản chính cho cơ quan chứng thực để đối chiếu khi làm thủ tục thực tế. Thời hạn hiển thị là thông số thực hành.', 2, N'GIU_NGUYEN'),
    ('khoi-dau-xac-nhan', N'TH-03', N'Xác nhận thông tin về cư trú', N'Hồ sơ thực hành về kê khai trực tuyến theo các trường thông tin của CT01. Bản xem lại PDF minh họa nội dung kê khai. Kết quả tham chiếu CT07. Thời hạn hiển thị là thông số thực hành.', 5, N'TAM_DUNG');

DECLARE @ThemThuTuc TABLE (Id varchar(50) PRIMARY KEY);
INSERT dbo.ThuTuc (ThuTucId, MaThuTuc, TenThuTuc, MoTa, SoNgaySLA, DonViSLA, SoGioPhanHoi, QuyTacBoSung, BoPhanChuyenMonId, HoatDong)
OUTPUT inserted.ThuTucId INTO @ThemThuTuc
SELECT Id, Ma, Ten, MoTa, SoNgay, N'NGAY_LAM_VIEC', 8, QuyTac, @ChuyenMon, 1 FROM @ThuTuc AS source
WHERE NOT EXISTS (SELECT 1 FROM dbo.ThuTuc AS target WHERE target.MaThuTuc = source.Ma);

INSERT dbo.ThanhPhanYeuCau (ThanhPhanId, ThuTucId, TenThanhPhan, BatBuoc, HuongDan, ThuTu)
SELECT item.Id, item.ThuTucId, item.Ten, item.BatBuoc, item.HuongDan, item.ThuTu
FROM (VALUES
    ('th-01-don', 'khoi-dau-ho-tich', N'Tờ khai điện tử đề nghị cấp bản sao trích lục hộ tịch', 1, N'Kê khai người yêu cầu, loại sự kiện hộ tịch, nơi đăng ký, số bản và cách nhận. Tệp PDF thực hành là bản lưu nội dung đã kê khai.', 1),
    ('th-01-can-cu', 'khoi-dau-ho-tich', N'Giấy tờ đối chiếu khi cơ quan yêu cầu', 0, N'Đính kèm giấy tờ theo tình huống, như giấy tờ chứng minh nơi cư trú khi cơ quan chưa khai thác được dữ liệu. Chỉ dùng bản chụp rõ và đủ trang.', 2),
    ('th-02-don', 'khoi-dau-chung-thuc', N'Bản số hóa giấy tờ dùng để đối chiếu bản chính', 1, N'Tải bản quét rõ, đủ trang để trình chiếu bước kiểm tra. Thực tế cần xuất trình bản chính để đối chiếu theo hướng dẫn của cơ quan chứng thực.', 1),
    ('th-02-ban-chup', 'khoi-dau-chung-thuc', N'Bản sao giấy tờ đề nghị chứng thực', 0, N'Đính kèm bản sao nếu đã chuẩn bị. Trường hợp chưa có bản sao, cơ quan có thể chụp từ bản chính nếu có phương tiện.', 2),
    ('th-03-don', 'khoi-dau-xac-nhan', N'Bản xem lại nội dung kê khai điện tử (CT01)', 0, N'Khi nộp trực tuyến trên cổng chính thức, khai trực tiếp trên biểu mẫu điện tử. Bản PDF đi kèm chỉ để xem lại nội dung đã kê khai trong hồ sơ thực hành.', 1),
    ('th-03-tai-lieu', 'khoi-dau-xac-nhan', N'Tài liệu hỗ trợ xác minh theo yêu cầu', 0, N'Chỉ dùng khi có một yêu cầu bổ sung cụ thể trong hồ sơ thực hành.', 2)
) AS item (Id, ThuTucId, Ten, BatBuoc, HuongDan, ThuTu)
JOIN @ThemThuTuc AS insertedProcedure ON insertedProcedure.Id = item.ThuTucId;

COMMIT TRANSACTION;

SELECT MaThuTuc, TenThuTuc FROM dbo.ThuTuc WHERE MaThuTuc IN (N'TH-01', N'TH-02', N'TH-03') ORDER BY MaThuTuc;
