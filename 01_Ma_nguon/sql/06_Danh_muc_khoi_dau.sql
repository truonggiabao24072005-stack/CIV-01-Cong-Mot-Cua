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
    ('khoi-dau-ho-tich', N'TH-01', N'Cấp bản sao trích lục hộ tịch', N'Danh mục thực hành. Ghi rõ loại trích lục, người có thông tin hộ tịch và số bản cần cấp.', 3, N'TINH_LAI'),
    ('khoi-dau-chung-thuc', N'TH-02', N'Chứng thực bản sao từ bản chính', N'Danh mục thực hành. Chuẩn bị bản chụp giấy tờ cần đối chiếu và ghi rõ số lượng bản sao.', 2, N'GIU_NGUYEN'),
    ('khoi-dau-xac-nhan', N'TH-03', N'Xác nhận thông tin theo đề nghị', N'Danh mục thực hành. Nêu rõ nội dung cần xác nhận và đính kèm tài liệu có liên quan.', 5, N'TAM_DUNG');

DECLARE @ThemThuTuc TABLE (Id varchar(50) PRIMARY KEY);
INSERT dbo.ThuTuc (ThuTucId, MaThuTuc, TenThuTuc, MoTa, SoNgaySLA, DonViSLA, SoGioPhanHoi, QuyTacBoSung, BoPhanChuyenMonId, HoatDong)
OUTPUT inserted.ThuTucId INTO @ThemThuTuc
SELECT Id, Ma, Ten, MoTa, SoNgay, N'NGAY_LAM_VIEC', 8, QuyTac, @ChuyenMon, 1 FROM @ThuTuc AS source
WHERE NOT EXISTS (SELECT 1 FROM dbo.ThuTuc AS target WHERE target.MaThuTuc = source.Ma);

INSERT dbo.ThanhPhanYeuCau (ThanhPhanId, ThuTucId, TenThanhPhan, BatBuoc, HuongDan, ThuTu)
SELECT item.Id, item.ThuTucId, item.Ten, item.BatBuoc, item.HuongDan, item.ThuTu
FROM (VALUES
    ('th-01-don', 'khoi-dau-ho-tich', N'Đơn đề nghị cấp trích lục', 1, N'Ghi loại trích lục và số bản cần cấp.', 1),
    ('th-01-can-cu', 'khoi-dau-ho-tich', N'Tài liệu giúp tra cứu thông tin hộ tịch', 1, N'Dùng tệp thực hành có thông tin sự kiện để đối chiếu.', 2),
    ('th-02-don', 'khoi-dau-chung-thuc', N'Phiếu đề nghị chứng thực', 1, N'Liệt kê giấy tờ và số bản sao đề nghị chứng thực.', 1),
    ('th-02-ban-chup', 'khoi-dau-chung-thuc', N'Bản chụp giấy tờ cần chứng thực', 1, N'Đính kèm đủ trang có nội dung cần đối chiếu.', 2),
    ('th-03-don', 'khoi-dau-xac-nhan', N'Đơn đề nghị xác nhận', 1, N'Ghi nội dung, mục đích và thông tin cần xác nhận.', 1),
    ('th-03-tai-lieu', 'khoi-dau-xac-nhan', N'Tài liệu liên quan', 0, N'Đính kèm tài liệu hỗ trợ nếu có.', 2)
) AS item (Id, ThuTucId, Ten, BatBuoc, HuongDan, ThuTu)
JOIN @ThemThuTuc AS insertedProcedure ON insertedProcedure.Id = item.ThuTucId;

COMMIT TRANSACTION;

SELECT MaThuTuc, TenThuTuc FROM dbo.ThuTuc WHERE MaThuTuc IN (N'TH-01', N'TH-02', N'TH-03') ORDER BY MaThuTuc;
