-- Mau quan tri trong SSMS; doi duong dan theo may SQL Server.
-- Tai khoan dich vu SQL Server can quyen ghi vao thu muc da chuan bi.
USE [master];
GO
DECLARE @Tep nvarchar(4000)=N'C:\SQLBackup\CongMotCua.bak';
BACKUP DATABASE [CongMotCua_VanHanh]
TO DISK=@Tep WITH CHECKSUM, STATS=10;
RESTORE VERIFYONLY FROM DISK=@Tep WITH CHECKSUM;
GO
-- Truoc khi khoi phuc, doc logical filename:
-- RESTORE FILELISTONLY FROM DISK=N'C:\SQLBackup\CongMotCua.bak';
-- Khoi phuc vao TEN MOI, thay ten logical va duong dan cho dung:
-- RESTORE DATABASE [CongMotCua_KhoiPhuc]
-- FROM DISK=N'C:\SQLBackup\CongMotCua.bak'
-- WITH MOVE N'<logical_data>' TO N'C:\SQLData\CongMotCua_KhoiPhuc.mdf',
--      MOVE N'<logical_log>' TO N'C:\SQLData\CongMotCua_KhoiPhuc_log.ldf',
--      CHECKSUM, STATS=10;
-- Sao luu va khoi phuc kem .data/files va .data/identity cung dot.
