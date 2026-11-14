USE [CongMotCua_VanHanh];
GO
CREATE OR ALTER VIEW dbo.v_HoSoTheoDoi AS
SELECT h.HoSoId,h.MaGui,h.MaHoSo,h.TrangThai,h.ChuHoSoId,n.HoTen AS NguoiNop,
       t.TenThuTuc,h.NgayNop,h.NgayTiepNhan,h.HanTraGoc,h.HanTra,h.NgayKetThuc,h.RowVersion,
       CASE WHEN h.HanTra IS NULL THEN N'Chua tinh SLA'
            WHEN h.NgayKetThuc IS NOT NULL AND h.NgayKetThuc>h.HanTra THEN N'Hoan tat tre han'
            WHEN h.NgayKetThuc IS NOT NULL THEN N'Hoan tat dung han'
            WHEN h.HanTra<SYSUTCDATETIME() THEN N'Dang qua han' ELSE N'Dang trong han' END AS TinhTrangHan
FROM dbo.HoSo h JOIN dbo.NguoiDung n ON n.NguoiDungId=h.ChuHoSoId
JOIN dbo.ThuTuc t ON t.ThuTucId=h.ThuTucId;
GO
CREATE OR ALTER VIEW dbo.v_PhanCongHienTai AS
SELECT p.PhanCongId,p.HoSoId,h.MaHoSo,h.TrangThai,n.HoTen AS CanBoNhan,b.TenBoPhan,p.BatDauLuc,h.HanTra
FROM dbo.PhanCongXuLy p JOIN dbo.NguoiDung n ON n.NguoiDungId=p.CanBoNhanId
JOIN dbo.BoPhan b ON b.BoPhanId=p.BoPhanNhanId JOIN dbo.HoSo h ON h.HoSoId=p.HoSoId
WHERE p.KetThucLuc IS NULL;
GO
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeThuTuc @TuNgay date=NULL,@DenNgay date=NULL AS
BEGIN
 SET NOCOUNT ON;
 SELECT t.MaThuTuc,t.TenThuTuc,COUNT(h.HoSoId) AS TongHoSo,
 SUM(CASE WHEN h.TrangThai='DA_TRA_KET_QUA' THEN 1 ELSE 0 END) AS DaTra,
 SUM(CASE WHEN h.HanTra<SYSUTCDATETIME() AND h.NgayKetThuc IS NULL THEN 1 ELSE 0 END) AS DangQuaHan,
 SUM(CASE WHEN h.NgayKetThuc>h.HanTra THEN 1 ELSE 0 END) AS HoanTatTreHan
 FROM dbo.ThuTuc t LEFT JOIN dbo.HoSo h ON h.ThuTucId=t.ThuTucId AND h.TrangThai<>'BAN_NHAP'
 AND (@TuNgay IS NULL OR h.NgayNop>=@TuNgay) AND (@DenNgay IS NULL OR h.NgayNop<DATEADD(day,1,@DenNgay))
 GROUP BY t.MaThuTuc,t.TenThuTuc;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_LichSuHoSo @HoSoId varchar(50) AS
BEGIN
 SET NOCOUNT ON;
 SELECT l.ThoiDiem,n.HoTen,l.HanhDong,l.TrangThaiTruoc,l.TrangThaiSau,l.NoiDung,l.CongKhai
 FROM dbo.LichSuXuLy l JOIN dbo.NguoiDung n ON n.NguoiDungId=l.NguoiThucHienId
 WHERE l.HoSoId=@HoSoId ORDER BY l.ThoiDiem,l.SuKienId;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_TaiLieu_DungHoSoThuTuc ON dbo.TaiLieuHoSo AFTER INSERT,UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.HoSo h ON h.HoSoId=i.HoSoId
  JOIN dbo.ThanhPhanYeuCau t ON t.ThanhPhanId=i.ThanhPhanId WHERE h.ThuTucId<>t.ThuTucId)
  THROW 51001,N'Thanh phan tai lieu phai thuoc thu tuc cua ho so.',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.LanBoSung l ON l.LanId=i.LanId
  JOIN dbo.YeuCauBoSung y ON y.YeuCauId=l.YeuCauId WHERE i.HoSoId<>y.HoSoId)
  THROW 51002,N'Lan bo sung va tai lieu phai cung ho so.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_NoiDungBoSung_DungThanhPhan ON dbo.NoiDungBoSung AFTER INSERT,UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.YeuCauBoSung y ON y.YeuCauId=i.YeuCauId
  JOIN dbo.HoSo h ON h.HoSoId=y.HoSoId JOIN dbo.ThanhPhanYeuCau t ON t.ThanhPhanId=i.ThanhPhanId WHERE h.ThuTucId<>t.ThuTucId)
  THROW 51003,N'Noi dung bo sung phai dung thanh phan thu tuc.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_PhanCong_DungCanBo ON dbo.PhanCongXuLy AFTER INSERT,UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.NguoiDung n ON n.NguoiDungId=i.CanBoNhanId
  JOIN dbo.HoSo h ON h.HoSoId=i.HoSoId JOIN dbo.ThuTuc t ON t.ThuTucId=h.ThuTucId
  JOIN dbo.BoPhan b ON b.BoPhanId=i.BoPhanNhanId
  WHERE i.KetThucLuc IS NULL AND(n.VaiTro<>'GIAI_QUYET' OR n.HoatDong=0 OR b.HoatDong=0 OR ISNULL(n.BoPhanId,'')<>i.BoPhanNhanId OR i.BoPhanNhanId<>t.BoPhanChuyenMonId))
  THROW 51004,N'Phan cong phai dung can bo va bo phan chuyen mon.',1;
END;
GO
CREATE OR ALTER TRIGGER dbo.TR_LichSu_ChiThem ON dbo.LichSuXuLy INSTEAD OF UPDATE,DELETE AS
BEGIN
 THROW 51005,N'Lich su xu ly chi duoc them, khong sua hoac xoa truc tiep.',1;
END;
GO
IF DATABASE_PRINCIPAL_ID(N'cmc_bao_cao') IS NULL CREATE ROLE cmc_bao_cao;
IF DATABASE_PRINCIPAL_ID(N'cmc_ung_dung') IS NULL CREATE ROLE cmc_ung_dung;
GO
GRANT SELECT ON dbo.v_HoSoTheoDoi TO cmc_bao_cao;
GRANT SELECT ON dbo.v_PhanCongHienTai TO cmc_bao_cao;
GRANT EXECUTE ON dbo.sp_ThongKeThuTuc TO cmc_bao_cao;
GRANT EXECUTE ON dbo.sp_LichSuHoSo TO cmc_bao_cao;
GRANT SELECT,INSERT,UPDATE ON SCHEMA::dbo TO cmc_ung_dung;
-- CRUD danh muc co cau truc duoc thuc hien trong giao dich. Lich su khong duoc UPDATE/DELETE.
GRANT DELETE ON dbo.ThanhPhanYeuCau TO cmc_ung_dung;
DENY UPDATE,DELETE ON dbo.LichSuXuLy TO cmc_ung_dung;
GO

-- Bo sung rang buoc gioi han va thoi gian; 12 CHECK co san trong script 01.
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_ThuTuc_GioiHan')
    ALTER TABLE dbo.[ThuTuc] WITH CHECK ADD CONSTRAINT [CK_ThuTuc_GioiHan] CHECK (SoNgaySLA<=365 AND SoGioPhanHoi<=720);
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_TaiLieuHoSo_DungLuongToiDa')
    ALTER TABLE dbo.[TaiLieuHoSo] WITH CHECK ADD CONSTRAINT [CK_TaiLieuHoSo_DungLuongToiDa] CHECK (DungLuong<=10485760);
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_YeuCauBoSung_QuayLai')
    ALTER TABLE dbo.[YeuCauBoSung] WITH CHECK ADD CONSTRAINT [CK_YeuCauBoSung_QuayLai] CHECK (TrangThaiQuayLai IN('DA_GUI','DANG_GIAI_QUYET'));
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_LanBoSung_DanhGia')
    ALTER TABLE dbo.[LanBoSung] WITH CHECK ADD CONSTRAINT [CK_LanBoSung_DanhGia] CHECK (DanhGia IS NULL OR DanhGia IN('DAT','CHUA_DAT'));
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_PhanCongXuLy_ThoiGian')
    ALTER TABLE dbo.[PhanCongXuLy] WITH CHECK ADD CONSTRAINT [CK_PhanCongXuLy_ThoiGian] CHECK (KetThucLuc IS NULL OR KetThucLuc>=BatDauLuc);
GO
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_KetQuaHoSo_ThoiGian')
    ALTER TABLE dbo.[KetQuaHoSo] WITH CHECK ADD CONSTRAINT [CK_KetQuaHoSo_ThoiGian] CHECK (PhatHanhLuc IS NULL OR PhatHanhLuc>=TaoLuc);
GO
-- Dong bo phien ban khoi dau la 1; giao dich DDL giu nguyen rang buoc neu nang cap loi.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_HoSo_Version')
    ALTER TABLE dbo.HoSo DROP CONSTRAINT CK_HoSo_Version;
ALTER TABLE dbo.HoSo WITH CHECK ADD CONSTRAINT CK_HoSo_Version CHECK ([RowVersion]>=1);
COMMIT TRANSACTION;
GO
