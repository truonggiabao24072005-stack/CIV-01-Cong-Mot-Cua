-- CIV-01 | Cổng một cửa theo dõi hồ sơ dịch vụ công
-- Mở tệp trong SQL Server Management Studio (SSMS), kết nối SQL Server và chọn Execute.
-- Tạo database CongMotCua_VanHanh nếu chưa có. Tạo 13 bảng và danh mục thực hành khi database trống.
-- Giữ nguyên dữ liệu hiện có. Với lược đồ đang có, chỉ cập nhật đối tượng quản trị.

USE [master];
GO
IF DB_ID(N'CongMotCua_VanHanh') IS NULL
    EXEC(N'CREATE DATABASE [CongMotCua_VanHanh]');
GO
USE [CongMotCua_VanHanh];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Lược đồ và chỉ mục
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    BEGIN
        CREATE TABLE [BoPhan] (
            [BoPhanId] varchar(50) NOT NULL,
            [MaBoPhan] nvarchar(255) NOT NULL,
            [TenBoPhan] nvarchar(255) NOT NULL,
            [HoatDong] bit NOT NULL,
            CONSTRAINT [PK_BoPhan] PRIMARY KEY ([BoPhanId])
        );



        CREATE TABLE [NguoiDung] (
            [NguoiDungId] varchar(50) NOT NULL,
            [Issuer] nvarchar(255) NOT NULL,
            [Subject] nvarchar(255) NOT NULL,
            [HoTen] nvarchar(255) NOT NULL,
            [SoDienThoai] nvarchar(255) NULL,
            [Email] nvarchar(255) NULL,
            [BoPhanId] varchar(50) NULL,
            [HoatDong] bit NOT NULL,
            [VaiTro] nvarchar(255) NOT NULL,
            CONSTRAINT [PK_NguoiDung] PRIMARY KEY ([NguoiDungId]),
            CONSTRAINT [CK_NguoiDung_VaiTro] CHECK ([VaiTro] IN ('NGUOI_NOP','TIEP_NHAN','GIAI_QUYET')),
            CONSTRAINT [FK_NguoiDung_BoPhan_BoPhanId] FOREIGN KEY ([BoPhanId]) REFERENCES [BoPhan] ([BoPhanId]) ON DELETE NO ACTION
        );



        CREATE TABLE [ThuTuc] (
            [ThuTucId] varchar(50) NOT NULL,
            [MaThuTuc] nvarchar(255) NOT NULL,
            [TenThuTuc] nvarchar(255) NOT NULL,
            [MoTa] nvarchar(max) NOT NULL,
            [SoNgaySLA] int NOT NULL,
            [DonViSLA] nvarchar(255) NOT NULL,
            [SoGioPhanHoi] int NOT NULL,
            [QuyTacBoSung] nvarchar(255) NOT NULL,
            [BoPhanChuyenMonId] varchar(50) NOT NULL,
            [HoatDong] bit NOT NULL,
            CONSTRAINT [PK_ThuTuc] PRIMARY KEY ([ThuTucId]),
            CONSTRAINT [CK_ThuTuc_DonVi] CHECK ([DonViSLA] IN ('NGAY_LICH','NGAY_LAM_VIEC')),
            CONSTRAINT [CK_ThuTuc_QuyTac] CHECK ([QuyTacBoSung] IN ('TINH_LAI','GIU_NGUYEN','TAM_DUNG')),
            CONSTRAINT [CK_ThuTuc_SLA] CHECK ([SoNgaySLA]>0 AND [SoGioPhanHoi]>0),
            CONSTRAINT [FK_ThuTuc_BoPhan_BoPhanChuyenMonId] FOREIGN KEY ([BoPhanChuyenMonId]) REFERENCES [BoPhan] ([BoPhanId]) ON DELETE NO ACTION
        );



        CREATE TABLE [HoSo] (
            [HoSoId] varchar(50) NOT NULL,
            [MaGui] nvarchar(255) NULL,
            [MaHoSo] nvarchar(255) NULL,
            [ChuHoSoId] varchar(50) NOT NULL,
            [ThuTucId] varchar(50) NOT NULL,
            [ThongTinNguoiNop] nvarchar(max) NULL,
            [NoiDungDeNghi] nvarchar(max) NULL,
            [TrangThai] nvarchar(255) NOT NULL,
            [NgayNop] datetime2 NULL,
            [NgayTiepNhan] datetime2 NULL,
            [MocTinhSLA] datetime2 NULL,
            [SoNgaySLA] int NULL,
            [DonViSLA] nvarchar(255) NULL,
            [QuyTacBoSung] nvarchar(255) NULL,
            [LichApDung] nvarchar(max) NULL,
            [HanTraGoc] datetime2 NULL,
            [HanTra] datetime2 NULL,
            [NgayKetThuc] datetime2 NULL,
            [RowVersion] int NOT NULL,
            CONSTRAINT [PK_HoSo] PRIMARY KEY ([HoSoId]),
            CONSTRAINT [CK_HoSo_Han] CHECK ([HanTra] IS NULL OR ([NgayTiepNhan] IS NOT NULL AND [HanTra]>=[NgayTiepNhan])),
            CONSTRAINT [CK_HoSo_TrangThai] CHECK ([TrangThai] IN ('BAN_NHAP','DA_GUI','DA_TIEP_NHAN','DANG_GIAI_QUYET','CHO_BO_SUNG','DA_CO_KET_QUA','DA_TRA_KET_QUA','TU_CHOI_TIEP_NHAN','TU_CHOI_GIAI_QUYET')),
            CONSTRAINT [CK_HoSo_Version] CHECK ([RowVersion]>=1),
            CONSTRAINT [FK_HoSo_NguoiDung_ChuHoSoId] FOREIGN KEY ([ChuHoSoId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_HoSo_ThuTuc_ThuTucId] FOREIGN KEY ([ThuTucId]) REFERENCES [ThuTuc] ([ThuTucId]) ON DELETE NO ACTION
        );



        CREATE TABLE [ThanhPhanYeuCau] (
            [ThanhPhanId] varchar(50) NOT NULL,
            [ThuTucId] varchar(50) NOT NULL,
            [TenThanhPhan] nvarchar(255) NOT NULL,
            [BatBuoc] bit NOT NULL,
            [HuongDan] nvarchar(max) NOT NULL,
            [ThuTu] int NOT NULL,
            CONSTRAINT [PK_ThanhPhanYeuCau] PRIMARY KEY ([ThanhPhanId]),
            CONSTRAINT [FK_ThanhPhanYeuCau_ThuTuc_ThuTucId] FOREIGN KEY ([ThuTucId]) REFERENCES [ThuTuc] ([ThuTucId]) ON DELETE NO ACTION
        );



        CREATE TABLE [KetQuaHoSo] (
            [KetQuaId] varchar(50) NOT NULL,
            [HoSoId] varchar(50) NOT NULL,
            [LoaiKetQua] nvarchar(255) NOT NULL,
            [NoiDungKetLuan] nvarchar(max) NOT NULL,
            [LyDoTuChoi] nvarchar(max) NULL,
            [TepKey] nvarchar(255) NULL,
            [NguoiTaoId] varchar(50) NOT NULL,
            [TaoLuc] datetime2 NOT NULL,
            [NguoiPhatHanhId] varchar(50) NULL,
            [PhatHanhLuc] datetime2 NULL,
            CONSTRAINT [PK_KetQuaHoSo] PRIMARY KEY ([KetQuaId]),
            CONSTRAINT [CK_KetQua_Loai] CHECK ([LoaiKetQua] IN ('THANH_CONG','TU_CHOI_TIEP_NHAN','TU_CHOI_GIAI_QUYET') AND ([LoaiKetQua]='THANH_CONG' OR LEN(LTRIM(RTRIM(ISNULL([LyDoTuChoi],''))))>0)),
            CONSTRAINT [FK_KetQuaHoSo_HoSo_HoSoId] FOREIGN KEY ([HoSoId]) REFERENCES [HoSo] ([HoSoId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_KetQuaHoSo_NguoiDung_NguoiPhatHanhId] FOREIGN KEY ([NguoiPhatHanhId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_KetQuaHoSo_NguoiDung_NguoiTaoId] FOREIGN KEY ([NguoiTaoId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION
        );



        CREATE TABLE [LichSuXuLy] (
            [SuKienId] varchar(50) NOT NULL,
            [HoSoId] varchar(50) NOT NULL,
            [NguoiThucHienId] varchar(50) NOT NULL,
            [HanhDong] nvarchar(255) NOT NULL,
            [TrangThaiTruoc] nvarchar(255) NOT NULL,
            [TrangThaiSau] nvarchar(255) NOT NULL,
            [NoiDung] nvarchar(max) NOT NULL,
            [CongKhai] bit NOT NULL,
            [ThoiDiem] datetime2 NOT NULL,
            CONSTRAINT [PK_LichSuXuLy] PRIMARY KEY ([SuKienId]),
            CONSTRAINT [FK_LichSuXuLy_HoSo_HoSoId] FOREIGN KEY ([HoSoId]) REFERENCES [HoSo] ([HoSoId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_LichSuXuLy_NguoiDung_NguoiThucHienId] FOREIGN KEY ([NguoiThucHienId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION
        );



        CREATE TABLE [PhanCongXuLy] (
            [PhanCongId] varchar(50) NOT NULL,
            [HoSoId] varchar(50) NOT NULL,
            [NguoiChuyenId] varchar(50) NOT NULL,
            [CanBoNhanId] varchar(50) NOT NULL,
            [BoPhanNhanId] varchar(50) NOT NULL,
            [BatDauLuc] datetime2 NOT NULL,
            [KetThucLuc] datetime2 NULL,
            [NoiDungChuyen] nvarchar(max) NOT NULL,
            CONSTRAINT [PK_PhanCongXuLy] PRIMARY KEY ([PhanCongId]),
            CONSTRAINT [FK_PhanCongXuLy_BoPhan_BoPhanNhanId] FOREIGN KEY ([BoPhanNhanId]) REFERENCES [BoPhan] ([BoPhanId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_PhanCongXuLy_HoSo_HoSoId] FOREIGN KEY ([HoSoId]) REFERENCES [HoSo] ([HoSoId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_PhanCongXuLy_NguoiDung_CanBoNhanId] FOREIGN KEY ([CanBoNhanId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_PhanCongXuLy_NguoiDung_NguoiChuyenId] FOREIGN KEY ([NguoiChuyenId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION
        );



        CREATE TABLE [YeuCauBoSung] (
            [YeuCauId] varchar(50) NOT NULL,
            [HoSoId] varchar(50) NOT NULL,
            [NguoiYeuCauId] varchar(50) NOT NULL,
            [TrangThai] nvarchar(255) NOT NULL,
            [TrangThaiQuayLai] nvarchar(255) NOT NULL,
            [TaoLuc] datetime2 NOT NULL,
            [DongLuc] datetime2 NULL,
            CONSTRAINT [PK_YeuCauBoSung] PRIMARY KEY ([YeuCauId]),
            CONSTRAINT [CK_YeuCau_TrangThai] CHECK ([TrangThai] IN ('DANG_CHO','DA_GUI','DA_DAP_UNG')),
            CONSTRAINT [FK_YeuCauBoSung_HoSo_HoSoId] FOREIGN KEY ([HoSoId]) REFERENCES [HoSo] ([HoSoId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_YeuCauBoSung_NguoiDung_NguoiYeuCauId] FOREIGN KEY ([NguoiYeuCauId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION
        );



        CREATE TABLE [LanBoSung] (
            [LanId] varchar(50) NOT NULL,
            [YeuCauId] varchar(50) NOT NULL,
            [SoLan] int NOT NULL,
            [NoiDungGiaiTrinh] nvarchar(max) NULL,
            [ThongTinDieuChinh] nvarchar(max) NULL,
            [GuiLuc] datetime2 NOT NULL,
            [DanhGia] nvarchar(255) NULL,
            [NhanXet] nvarchar(max) NULL,
            [NguoiDanhGiaId] varchar(50) NULL,
            [DanhGiaLuc] datetime2 NULL,
            CONSTRAINT [PK_LanBoSung] PRIMARY KEY ([LanId]),
            CONSTRAINT [CK_LanBoSung_SoLan] CHECK ([SoLan]>0),
            CONSTRAINT [FK_LanBoSung_NguoiDung_NguoiDanhGiaId] FOREIGN KEY ([NguoiDanhGiaId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_LanBoSung_YeuCauBoSung_YeuCauId] FOREIGN KEY ([YeuCauId]) REFERENCES [YeuCauBoSung] ([YeuCauId]) ON DELETE NO ACTION
        );



        CREATE TABLE [NoiDungBoSung] (
            [NoiDungId] varchar(50) NOT NULL,
            [YeuCauId] varchar(50) NOT NULL,
            [ThanhPhanId] varchar(50) NULL,
            [TruongThongTin] nvarchar(255) NULL,
            [VanDe] nvarchar(max) NOT NULL,
            [LyDo] nvarchar(max) NOT NULL,
            [HuongDan] nvarchar(max) NOT NULL,
            CONSTRAINT [PK_NoiDungBoSung] PRIMARY KEY ([NoiDungId]),
            CONSTRAINT [FK_NoiDungBoSung_ThanhPhanYeuCau_ThanhPhanId] FOREIGN KEY ([ThanhPhanId]) REFERENCES [ThanhPhanYeuCau] ([ThanhPhanId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_NoiDungBoSung_YeuCauBoSung_YeuCauId] FOREIGN KEY ([YeuCauId]) REFERENCES [YeuCauBoSung] ([YeuCauId]) ON DELETE NO ACTION
        );



        CREATE TABLE [TaiLieuHoSo] (
            [TaiLieuId] varchar(50) NOT NULL,
            [HoSoId] varchar(50) NOT NULL,
            [ThanhPhanId] varchar(50) NOT NULL,
            [LanId] varchar(50) NULL,
            [SoPhienBan] int NOT NULL,
            [TenTep] nvarchar(255) NOT NULL,
            [LoaiTep] nvarchar(255) NOT NULL,
            [DungLuong] int NOT NULL,
            [TepKey] nvarchar(255) NOT NULL,
            [HienHanh] bit NOT NULL,
            [TaoLuc] datetime2 NOT NULL,
            CONSTRAINT [PK_TaiLieuHoSo] PRIMARY KEY ([TaiLieuId]),
            CONSTRAINT [CK_TaiLieu_PhienBan] CHECK ([SoPhienBan]>0 AND [DungLuong]>0),
            CONSTRAINT [FK_TaiLieuHoSo_HoSo_HoSoId] FOREIGN KEY ([HoSoId]) REFERENCES [HoSo] ([HoSoId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_TaiLieuHoSo_LanBoSung_LanId] FOREIGN KEY ([LanId]) REFERENCES [LanBoSung] ([LanId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_TaiLieuHoSo_ThanhPhanYeuCau_ThanhPhanId] FOREIGN KEY ([ThanhPhanId]) REFERENCES [ThanhPhanYeuCau] ([ThanhPhanId]) ON DELETE NO ACTION
        );



        CREATE TABLE [KiemTraThanhPhan] (
            [KiemTraId] varchar(50) NOT NULL,
            [TaiLieuId] varchar(50) NOT NULL,
            [CanBoId] varchar(50) NOT NULL,
            [KetQua] nvarchar(255) NOT NULL,
            [NhanXet] nvarchar(max) NULL,
            [ThoiDiem] datetime2 NOT NULL,
            CONSTRAINT [PK_KiemTraThanhPhan] PRIMARY KEY ([KiemTraId]),
            CONSTRAINT [CK_KiemTra_KetQua] CHECK ([KetQua] IN ('DAT','CHUA_DAT')),
            CONSTRAINT [FK_KiemTraThanhPhan_NguoiDung_CanBoId] FOREIGN KEY ([CanBoId]) REFERENCES [NguoiDung] ([NguoiDungId]) ON DELETE NO ACTION,
            CONSTRAINT [FK_KiemTraThanhPhan_TaiLieuHoSo_TaiLieuId] FOREIGN KEY ([TaiLieuId]) REFERENCES [TaiLieuHoSo] ([TaiLieuId]) ON DELETE NO ACTION
        );



        CREATE UNIQUE INDEX [IX_BoPhan_MaBoPhan] ON [BoPhan] ([MaBoPhan]);



        CREATE INDEX [IX_HoSo_ChuHoSoId_NgayNop] ON [HoSo] ([ChuHoSoId], [NgayNop]);



        CREATE UNIQUE INDEX [IX_HoSo_MaGui] ON [HoSo] ([MaGui]) WHERE [MaGui] IS NOT NULL;



        CREATE UNIQUE INDEX [IX_HoSo_MaHoSo] ON [HoSo] ([MaHoSo]) WHERE [MaHoSo] IS NOT NULL;



        CREATE INDEX [IX_HoSo_ThuTucId] ON [HoSo] ([ThuTucId]);



        CREATE INDEX [IX_HoSo_TrangThai_HanTra] ON [HoSo] ([TrangThai], [HanTra]);



        CREATE UNIQUE INDEX [IX_KetQuaHoSo_HoSoId] ON [KetQuaHoSo] ([HoSoId]);



        CREATE INDEX [IX_KetQuaHoSo_NguoiPhatHanhId] ON [KetQuaHoSo] ([NguoiPhatHanhId]);



        CREATE INDEX [IX_KetQuaHoSo_NguoiTaoId] ON [KetQuaHoSo] ([NguoiTaoId]);



        CREATE INDEX [IX_KiemTraThanhPhan_CanBoId] ON [KiemTraThanhPhan] ([CanBoId]);



        CREATE INDEX [IX_KiemTraThanhPhan_TaiLieuId_ThoiDiem] ON [KiemTraThanhPhan] ([TaiLieuId], [ThoiDiem]);



        CREATE INDEX [IX_LanBoSung_NguoiDanhGiaId] ON [LanBoSung] ([NguoiDanhGiaId]);



        CREATE UNIQUE INDEX [IX_LanBoSung_YeuCauId_SoLan] ON [LanBoSung] ([YeuCauId], [SoLan]);



        CREATE INDEX [IX_LichSuXuLy_HoSoId_ThoiDiem] ON [LichSuXuLy] ([HoSoId], [ThoiDiem]);



        CREATE INDEX [IX_LichSuXuLy_NguoiThucHienId] ON [LichSuXuLy] ([NguoiThucHienId]);



        CREATE INDEX [IX_NguoiDung_BoPhanId] ON [NguoiDung] ([BoPhanId]);



        CREATE UNIQUE INDEX [IX_NguoiDung_Issuer_Subject] ON [NguoiDung] ([Issuer], [Subject]);



        CREATE INDEX [IX_NoiDungBoSung_ThanhPhanId] ON [NoiDungBoSung] ([ThanhPhanId]);



        CREATE INDEX [IX_NoiDungBoSung_YeuCauId] ON [NoiDungBoSung] ([YeuCauId]);



        CREATE INDEX [IX_PhanCongXuLy_BoPhanNhanId] ON [PhanCongXuLy] ([BoPhanNhanId]);



        CREATE INDEX [IX_PhanCongXuLy_CanBoNhanId] ON [PhanCongXuLy] ([CanBoNhanId]);



        CREATE UNIQUE INDEX [IX_PhanCongXuLy_HoSoId] ON [PhanCongXuLy] ([HoSoId]) WHERE [KetThucLuc] IS NULL;



        CREATE INDEX [IX_PhanCongXuLy_NguoiChuyenId] ON [PhanCongXuLy] ([NguoiChuyenId]);



        CREATE UNIQUE INDEX [IX_TaiLieuHoSo_HoSoId_ThanhPhanId] ON [TaiLieuHoSo] ([HoSoId], [ThanhPhanId]) WHERE [HienHanh] = 1;



        CREATE UNIQUE INDEX [IX_TaiLieuHoSo_HoSoId_ThanhPhanId_SoPhienBan] ON [TaiLieuHoSo] ([HoSoId], [ThanhPhanId], [SoPhienBan]);



        CREATE INDEX [IX_TaiLieuHoSo_LanId] ON [TaiLieuHoSo] ([LanId]);



        CREATE UNIQUE INDEX [IX_TaiLieuHoSo_TepKey] ON [TaiLieuHoSo] ([TepKey]);



        CREATE INDEX [IX_TaiLieuHoSo_ThanhPhanId] ON [TaiLieuHoSo] ([ThanhPhanId]);



        CREATE INDEX [IX_ThanhPhanYeuCau_ThuTucId] ON [ThanhPhanYeuCau] ([ThuTucId]);



        CREATE INDEX [IX_ThuTuc_BoPhanChuyenMonId] ON [ThuTuc] ([BoPhanChuyenMonId]);



        CREATE UNIQUE INDEX [IX_ThuTuc_MaThuTuc] ON [ThuTuc] ([MaThuTuc]);



        CREATE UNIQUE INDEX [IX_YeuCauBoSung_HoSoId] ON [YeuCauBoSung] ([HoSoId]) WHERE [DongLuc] IS NULL;



        CREATE INDEX [IX_YeuCauBoSung_NguoiYeuCauId] ON [YeuCauBoSung] ([NguoiYeuCauId]);

    END
    ELSE IF (SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID(N'dbo')
             AND name IN (N'BoPhan', N'NguoiDung', N'ThuTuc', N'HoSo', N'ThanhPhanYeuCau', N'KetQuaHoSo', N'LichSuXuLy', N'PhanCongXuLy', N'YeuCauBoSung', N'LanBoSung', N'NoiDungBoSung', N'TaiLieuHoSo', N'KiemTraThanhPhan')) <> 13
        THROW 51031, N'Lược đồ hiện có chưa đủ 13 bảng của CIV-01.', 1;

    -- BEGIN QUAN_TRI
    EXEC sys.sp_executesql N'CREATE OR ALTER VIEW dbo.v_HoSoTheoDoi AS
    SELECT h.HoSoId,h.MaGui,h.MaHoSo,h.TrangThai,h.ChuHoSoId,n.HoTen AS NguoiNop,
           t.TenThuTuc,h.NgayNop,h.NgayTiepNhan,h.HanTraGoc,h.HanTra,h.NgayKetThuc,h.RowVersion,
           CASE WHEN h.HanTra IS NULL THEN N''Chua tinh SLA''
                WHEN h.NgayKetThuc IS NOT NULL AND h.NgayKetThuc>h.HanTra THEN N''Hoan tat tre han''
                WHEN h.NgayKetThuc IS NOT NULL THEN N''Hoan tat dung han''
                WHEN h.HanTra<SYSUTCDATETIME() THEN N''Dang qua han'' ELSE N''Dang trong han'' END AS TinhTrangHan
    FROM dbo.HoSo h JOIN dbo.NguoiDung n ON n.NguoiDungId=h.ChuHoSoId
    JOIN dbo.ThuTuc t ON t.ThuTucId=h.ThuTucId;';
    EXEC sys.sp_executesql N'CREATE OR ALTER VIEW dbo.v_PhanCongHienTai AS
    SELECT p.PhanCongId,p.HoSoId,h.MaHoSo,h.TrangThai,n.HoTen AS CanBoNhan,b.TenBoPhan,p.BatDauLuc,h.HanTra
    FROM dbo.PhanCongXuLy p JOIN dbo.NguoiDung n ON n.NguoiDungId=p.CanBoNhanId
    JOIN dbo.BoPhan b ON b.BoPhanId=p.BoPhanNhanId JOIN dbo.HoSo h ON h.HoSoId=p.HoSoId
    WHERE p.KetThucLuc IS NULL;';
    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_ThongKeThuTuc @TuNgay date=NULL,@DenNgay date=NULL AS
    BEGIN
     SET NOCOUNT ON;
     SELECT t.MaThuTuc,t.TenThuTuc,COUNT(h.HoSoId) AS TongHoSo,
     SUM(CASE WHEN h.TrangThai=''DA_TRA_KET_QUA'' THEN 1 ELSE 0 END) AS DaTra,
     SUM(CASE WHEN h.HanTra<SYSUTCDATETIME() AND h.NgayKetThuc IS NULL THEN 1 ELSE 0 END) AS DangQuaHan,
     SUM(CASE WHEN h.NgayKetThuc>h.HanTra THEN 1 ELSE 0 END) AS HoanTatTreHan
     FROM dbo.ThuTuc t LEFT JOIN dbo.HoSo h ON h.ThuTucId=t.ThuTucId AND h.TrangThai<>''BAN_NHAP''
     AND (@TuNgay IS NULL OR h.NgayNop>=@TuNgay) AND (@DenNgay IS NULL OR h.NgayNop<DATEADD(day,1,@DenNgay))
     GROUP BY t.MaThuTuc,t.TenThuTuc;
    END;';
    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_LichSuHoSo @HoSoId varchar(50) AS
    BEGIN
     SET NOCOUNT ON;
     SELECT l.ThoiDiem,n.HoTen,l.HanhDong,l.TrangThaiTruoc,l.TrangThaiSau,l.NoiDung,l.CongKhai
     FROM dbo.LichSuXuLy l JOIN dbo.NguoiDung n ON n.NguoiDungId=l.NguoiThucHienId
     WHERE l.HoSoId=@HoSoId ORDER BY l.ThoiDiem,l.SuKienId;
    END;';
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.TR_TaiLieu_DungHoSoThuTuc ON dbo.TaiLieuHoSo AFTER INSERT,UPDATE AS
    BEGIN
     SET NOCOUNT ON;
     IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.HoSo h ON h.HoSoId=i.HoSoId
      JOIN dbo.ThanhPhanYeuCau t ON t.ThanhPhanId=i.ThanhPhanId WHERE h.ThuTucId<>t.ThuTucId)
      THROW 51001,N''Thanh phan tai lieu phai thuoc thu tuc cua ho so.'',1;
     IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.LanBoSung l ON l.LanId=i.LanId
      JOIN dbo.YeuCauBoSung y ON y.YeuCauId=l.YeuCauId WHERE i.HoSoId<>y.HoSoId)
      THROW 51002,N''Lan bo sung va tai lieu phai cung ho so.'',1;
    END;';
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.TR_NoiDungBoSung_DungThanhPhan ON dbo.NoiDungBoSung AFTER INSERT,UPDATE AS
    BEGIN
     SET NOCOUNT ON;
     IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.YeuCauBoSung y ON y.YeuCauId=i.YeuCauId
      JOIN dbo.HoSo h ON h.HoSoId=y.HoSoId JOIN dbo.ThanhPhanYeuCau t ON t.ThanhPhanId=i.ThanhPhanId WHERE h.ThuTucId<>t.ThuTucId)
      THROW 51003,N''Noi dung bo sung phai dung thanh phan thu tuc.'',1;
    END;';
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.TR_PhanCong_DungCanBo ON dbo.PhanCongXuLy AFTER INSERT,UPDATE AS
    BEGIN
     SET NOCOUNT ON;
     IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.NguoiDung n ON n.NguoiDungId=i.CanBoNhanId
      JOIN dbo.HoSo h ON h.HoSoId=i.HoSoId JOIN dbo.ThuTuc t ON t.ThuTucId=h.ThuTucId
      JOIN dbo.BoPhan b ON b.BoPhanId=i.BoPhanNhanId
      WHERE i.KetThucLuc IS NULL AND(n.VaiTro<>''GIAI_QUYET'' OR n.HoatDong=0 OR b.HoatDong=0 OR ISNULL(n.BoPhanId,'''')<>i.BoPhanNhanId OR i.BoPhanNhanId<>t.BoPhanChuyenMonId))
      THROW 51004,N''Phan cong phai dung can bo va bo phan chuyen mon.'',1;
    END;';
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.TR_LichSu_ChiThem ON dbo.LichSuXuLy INSTEAD OF UPDATE,DELETE AS
    BEGIN
     THROW 51005,N''Lich su xu ly chi duoc them, khong sua hoac xoa truc tiep.'',1;
    END;';
    IF DATABASE_PRINCIPAL_ID(N'cmc_bao_cao') IS NULL CREATE ROLE cmc_bao_cao;
    IF DATABASE_PRINCIPAL_ID(N'cmc_ung_dung') IS NULL CREATE ROLE cmc_ung_dung;
    GRANT SELECT ON dbo.v_HoSoTheoDoi TO cmc_bao_cao;
    GRANT SELECT ON dbo.v_PhanCongHienTai TO cmc_bao_cao;
    GRANT EXECUTE ON dbo.sp_ThongKeThuTuc TO cmc_bao_cao;
    GRANT EXECUTE ON dbo.sp_LichSuHoSo TO cmc_bao_cao;
    GRANT SELECT,INSERT,UPDATE ON SCHEMA::dbo TO cmc_ung_dung;
    -- CRUD danh muc co cau truc duoc thuc hien trong giao dich. Lich su khong duoc UPDATE/DELETE.
    GRANT DELETE ON dbo.ThanhPhanYeuCau TO cmc_ung_dung;
    DENY UPDATE,DELETE ON dbo.LichSuXuLy TO cmc_ung_dung;
    -- Bo sung rang buoc gioi han va thoi gian; 12 CHECK co san trong script 01.
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_ThuTuc_GioiHan')
        ALTER TABLE dbo.[ThuTuc] WITH CHECK ADD CONSTRAINT [CK_ThuTuc_GioiHan] CHECK (SoNgaySLA<=365 AND SoGioPhanHoi<=720);
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_TaiLieuHoSo_DungLuongToiDa')
        ALTER TABLE dbo.[TaiLieuHoSo] WITH CHECK ADD CONSTRAINT [CK_TaiLieuHoSo_DungLuongToiDa] CHECK (DungLuong<=10485760);
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_YeuCauBoSung_QuayLai')
        ALTER TABLE dbo.[YeuCauBoSung] WITH CHECK ADD CONSTRAINT [CK_YeuCauBoSung_QuayLai] CHECK (TrangThaiQuayLai IN('DA_GUI','DANG_GIAI_QUYET'));
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_LanBoSung_DanhGia')
        ALTER TABLE dbo.[LanBoSung] WITH CHECK ADD CONSTRAINT [CK_LanBoSung_DanhGia] CHECK (DanhGia IS NULL OR DanhGia IN('DAT','CHUA_DAT'));
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_PhanCongXuLy_ThoiGian')
        ALTER TABLE dbo.[PhanCongXuLy] WITH CHECK ADD CONSTRAINT [CK_PhanCongXuLy_ThoiGian] CHECK (KetThucLuc IS NULL OR KetThucLuc>=BatDauLuc);
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_KetQuaHoSo_ThoiGian')
        ALTER TABLE dbo.[KetQuaHoSo] WITH CHECK ADD CONSTRAINT [CK_KetQuaHoSo_ThoiGian] CHECK (PhatHanhLuc IS NULL OR PhatHanhLuc>=TaoLuc);
    -- Dong bo phien ban khoi dau la 1; giao dich DDL giu nguyen rang buoc neu nang cap loi.
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_HoSo_Version')
        ALTER TABLE dbo.HoSo DROP CONSTRAINT CK_HoSo_Version;
    ALTER TABLE dbo.HoSo WITH CHECK ADD CONSTRAINT CK_HoSo_Version CHECK ([RowVersion]>=1);
    COMMIT TRANSACTION;
    -- END QUAN_TRI

    -- 3. Danh mục khởi đầu cho cơ sở dữ liệu chưa có dữ liệu nghiệp vụ
    IF NOT EXISTS (SELECT 1 FROM dbo.BoPhan)
       AND NOT EXISTS (SELECT 1 FROM dbo.ThuTuc)
       AND NOT EXISTS (SELECT 1 FROM dbo.NguoiDung)
       AND NOT EXISTS (SELECT 1 FROM dbo.HoSo)
    BEGIN
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
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- 4. Truy vấn quản trị tùy chọn
-- Bỏ dấu chú thích của truy vấn cần dùng và chạy riêng trong SSMS.
-- -- Danh sach 13 bang va so dong. Khong goi COUNT(*) tren moi bang lon de chi xem kich thuoc.
-- SELECT t.name AS Bang,SUM(p.rows) AS SoDong FROM sys.tables t
-- JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN(0,1) GROUP BY t.name ORDER BY t.name;
-- SELECT fk.name AS FK,OBJECT_NAME(fk.parent_object_id) AS BangCon,
-- COL_NAME(fc.parent_object_id,fc.parent_column_id) AS CotFK,OBJECT_NAME(fk.referenced_object_id) AS BangCha
-- FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=fk.object_id ORDER BY BangCon,CotFK;
-- SELECT OBJECT_NAME(object_id) AS Bang,name AS ChiMuc,is_unique,filter_definition FROM sys.indexes
-- WHERE object_id IN(SELECT object_id FROM sys.tables) AND is_unique=1 ORDER BY Bang;
-- SELECT * FROM dbo.v_HoSoTheoDoi ORDER BY NgayNop DESC;
-- SELECT * FROM dbo.v_PhanCongHienTai;
-- EXEC dbo.sp_ThongKeThuTuc;
-- -- Lich su mot ho so mau:
-- EXEC dbo.sp_LichSuHoSo @HoSoId='demo-hs-4';
-- -- Cau hinh hien tai cua thu tuc. Ho so da tiep nhan dung ban chup rieng.
-- SELECT ThuTucId,MaThuTuc,TenThuTuc,SoNgaySLA,DonViSLA,QuyTacBoSung,HoatDong FROM dbo.ThuTuc;
-- -- Doi chieu rang buoc so luong. Moi truy van sau phai tra ve 0 dong.
-- SELECT HoSoId,COUNT(*) AS SoLuong FROM dbo.PhanCongXuLy WHERE KetThucLuc IS NULL GROUP BY HoSoId HAVING COUNT(*)>1;
-- SELECT HoSoId,COUNT(*) AS SoLuong FROM dbo.YeuCauBoSung WHERE DongLuc IS NULL GROUP BY HoSoId HAVING COUNT(*)>1;
-- SELECT HoSoId,COUNT(*) AS SoLuong FROM dbo.KetQuaHoSo GROUP BY HoSoId HAVING COUNT(*)>1;
-- SELECT HoSoId,ThanhPhanId,COUNT(*) AS SoLuong FROM dbo.TaiLieuHoSo WHERE HienHanh=1 GROUP BY HoSoId,ThanhPhanId HAVING COUNT(*)>1;

-- 5. Sao lưu và khôi phục tùy chọn
-- -- Mau quan tri trong SSMS; doi duong dan theo may SQL Server.
-- -- Tai khoan dich vu SQL Server can quyen ghi vao thu muc da chuan bi.
-- USE [master];
-- GO
-- DECLARE @Tep nvarchar(4000)=N'C:\SQLBackup\CongMotCua.bak';
-- BACKUP DATABASE [CongMotCua_VanHanh]
-- TO DISK=@Tep WITH CHECKSUM, STATS=10;
-- RESTORE VERIFYONLY FROM DISK=@Tep WITH CHECKSUM;
-- GO
-- -- Truoc khi khoi phuc, doc logical filename:
-- -- RESTORE FILELISTONLY FROM DISK=N'C:\SQLBackup\CongMotCua.bak';
-- -- Khoi phuc vao TEN MOI, thay ten logical va duong dan cho dung:
-- -- RESTORE DATABASE [CongMotCua_KhoiPhuc]
-- -- FROM DISK=N'C:\SQLBackup\CongMotCua.bak'
-- -- WITH MOVE N'<logical_data>' TO N'C:\SQLData\CongMotCua_KhoiPhuc.mdf',
-- --      MOVE N'<logical_log>' TO N'C:\SQLData\CongMotCua_KhoiPhuc_log.ldf',
-- --      CHECKSUM, STATS=10;
-- -- Sao luu va khoi phuc kem .data/files va .data/identity cung dot.
