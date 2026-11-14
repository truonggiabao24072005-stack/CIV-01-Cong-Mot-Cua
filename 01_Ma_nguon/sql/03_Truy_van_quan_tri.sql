USE [CongMotCua_VanHanh];
GO
-- Danh sach 13 bang va so dong. Khong goi COUNT(*) tren moi bang lon de chi xem kich thuoc.
SELECT t.name AS Bang,SUM(p.rows) AS SoDong FROM sys.tables t
JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN(0,1) GROUP BY t.name ORDER BY t.name;
SELECT fk.name AS FK,OBJECT_NAME(fk.parent_object_id) AS BangCon,
COL_NAME(fc.parent_object_id,fc.parent_column_id) AS CotFK,OBJECT_NAME(fk.referenced_object_id) AS BangCha
FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=fk.object_id ORDER BY BangCon,CotFK;
SELECT OBJECT_NAME(object_id) AS Bang,name AS ChiMuc,is_unique,filter_definition FROM sys.indexes
WHERE object_id IN(SELECT object_id FROM sys.tables) AND is_unique=1 ORDER BY Bang;
SELECT * FROM dbo.v_HoSoTheoDoi ORDER BY NgayNop DESC;
SELECT * FROM dbo.v_PhanCongHienTai;
EXEC dbo.sp_ThongKeThuTuc;
-- Lich su mot ho so mau:
EXEC dbo.sp_LichSuHoSo @HoSoId='demo-hs-4';
-- Cau hinh hien tai cua thu tuc. Ho so da tiep nhan dung ban chup rieng.
SELECT ThuTucId,MaThuTuc,TenThuTuc,SoNgaySLA,DonViSLA,QuyTacBoSung,HoatDong FROM dbo.ThuTuc;
-- Doi chieu rang buoc so luong. Moi truy van sau phai tra ve 0 dong.
SELECT HoSoId,COUNT(*) AS SoLuong FROM dbo.PhanCongXuLy WHERE KetThucLuc IS NULL GROUP BY HoSoId HAVING COUNT(*)>1;
SELECT HoSoId,COUNT(*) AS SoLuong FROM dbo.YeuCauBoSung WHERE DongLuc IS NULL GROUP BY HoSoId HAVING COUNT(*)>1;
SELECT HoSoId,COUNT(*) AS SoLuong FROM dbo.KetQuaHoSo GROUP BY HoSoId HAVING COUNT(*)>1;
SELECT HoSoId,ThanhPhanId,COUNT(*) AS SoLuong FROM dbo.TaiLieuHoSo WHERE HienHanh=1 GROUP BY HoSoId,ThanhPhanId HAVING COUNT(*)>1;
GO
