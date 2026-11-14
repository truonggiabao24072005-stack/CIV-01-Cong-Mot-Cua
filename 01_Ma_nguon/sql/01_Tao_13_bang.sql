CREATE TABLE [BoPhan] (
    [BoPhanId] varchar(50) NOT NULL,
    [MaBoPhan] nvarchar(255) NOT NULL,
    [TenBoPhan] nvarchar(255) NOT NULL,
    [HoatDong] bit NOT NULL,
    CONSTRAINT [PK_BoPhan] PRIMARY KEY ([BoPhanId])
);
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


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
GO


CREATE UNIQUE INDEX [IX_BoPhan_MaBoPhan] ON [BoPhan] ([MaBoPhan]);
GO


CREATE INDEX [IX_HoSo_ChuHoSoId_NgayNop] ON [HoSo] ([ChuHoSoId], [NgayNop]);
GO


CREATE UNIQUE INDEX [IX_HoSo_MaGui] ON [HoSo] ([MaGui]) WHERE [MaGui] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_HoSo_MaHoSo] ON [HoSo] ([MaHoSo]) WHERE [MaHoSo] IS NOT NULL;
GO


CREATE INDEX [IX_HoSo_ThuTucId] ON [HoSo] ([ThuTucId]);
GO


CREATE INDEX [IX_HoSo_TrangThai_HanTra] ON [HoSo] ([TrangThai], [HanTra]);
GO


CREATE UNIQUE INDEX [IX_KetQuaHoSo_HoSoId] ON [KetQuaHoSo] ([HoSoId]);
GO


CREATE INDEX [IX_KetQuaHoSo_NguoiPhatHanhId] ON [KetQuaHoSo] ([NguoiPhatHanhId]);
GO


CREATE INDEX [IX_KetQuaHoSo_NguoiTaoId] ON [KetQuaHoSo] ([NguoiTaoId]);
GO


CREATE INDEX [IX_KiemTraThanhPhan_CanBoId] ON [KiemTraThanhPhan] ([CanBoId]);
GO


CREATE INDEX [IX_KiemTraThanhPhan_TaiLieuId_ThoiDiem] ON [KiemTraThanhPhan] ([TaiLieuId], [ThoiDiem]);
GO


CREATE INDEX [IX_LanBoSung_NguoiDanhGiaId] ON [LanBoSung] ([NguoiDanhGiaId]);
GO


CREATE UNIQUE INDEX [IX_LanBoSung_YeuCauId_SoLan] ON [LanBoSung] ([YeuCauId], [SoLan]);
GO


CREATE INDEX [IX_LichSuXuLy_HoSoId_ThoiDiem] ON [LichSuXuLy] ([HoSoId], [ThoiDiem]);
GO


CREATE INDEX [IX_LichSuXuLy_NguoiThucHienId] ON [LichSuXuLy] ([NguoiThucHienId]);
GO


CREATE INDEX [IX_NguoiDung_BoPhanId] ON [NguoiDung] ([BoPhanId]);
GO


CREATE UNIQUE INDEX [IX_NguoiDung_Issuer_Subject] ON [NguoiDung] ([Issuer], [Subject]);
GO


CREATE INDEX [IX_NoiDungBoSung_ThanhPhanId] ON [NoiDungBoSung] ([ThanhPhanId]);
GO


CREATE INDEX [IX_NoiDungBoSung_YeuCauId] ON [NoiDungBoSung] ([YeuCauId]);
GO


CREATE INDEX [IX_PhanCongXuLy_BoPhanNhanId] ON [PhanCongXuLy] ([BoPhanNhanId]);
GO


CREATE INDEX [IX_PhanCongXuLy_CanBoNhanId] ON [PhanCongXuLy] ([CanBoNhanId]);
GO


CREATE UNIQUE INDEX [IX_PhanCongXuLy_HoSoId] ON [PhanCongXuLy] ([HoSoId]) WHERE [KetThucLuc] IS NULL;
GO


CREATE INDEX [IX_PhanCongXuLy_NguoiChuyenId] ON [PhanCongXuLy] ([NguoiChuyenId]);
GO


CREATE UNIQUE INDEX [IX_TaiLieuHoSo_HoSoId_ThanhPhanId] ON [TaiLieuHoSo] ([HoSoId], [ThanhPhanId]) WHERE [HienHanh] = 1;
GO


CREATE UNIQUE INDEX [IX_TaiLieuHoSo_HoSoId_ThanhPhanId_SoPhienBan] ON [TaiLieuHoSo] ([HoSoId], [ThanhPhanId], [SoPhienBan]);
GO


CREATE INDEX [IX_TaiLieuHoSo_LanId] ON [TaiLieuHoSo] ([LanId]);
GO


CREATE UNIQUE INDEX [IX_TaiLieuHoSo_TepKey] ON [TaiLieuHoSo] ([TepKey]);
GO


CREATE INDEX [IX_TaiLieuHoSo_ThanhPhanId] ON [TaiLieuHoSo] ([ThanhPhanId]);
GO


CREATE INDEX [IX_ThanhPhanYeuCau_ThuTucId] ON [ThanhPhanYeuCau] ([ThuTucId]);
GO


CREATE INDEX [IX_ThuTuc_BoPhanChuyenMonId] ON [ThuTuc] ([BoPhanChuyenMonId]);
GO


CREATE UNIQUE INDEX [IX_ThuTuc_MaThuTuc] ON [ThuTuc] ([MaThuTuc]);
GO


CREATE UNIQUE INDEX [IX_YeuCauBoSung_HoSoId] ON [YeuCauBoSung] ([HoSoId]) WHERE [DongLuc] IS NULL;
GO


CREATE INDEX [IX_YeuCauBoSung_NguoiYeuCauId] ON [YeuCauBoSung] ([NguoiYeuCauId]);
GO



