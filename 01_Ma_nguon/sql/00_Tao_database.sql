-- Chay trong SSMS voi Windows Authentication. Script khong xoa database cu.
USE [master];
GO
IF DB_ID(N'CongMotCua_VanHanh') IS NULL
    EXEC(N'CREATE DATABASE [CongMotCua_VanHanh]');
GO
USE [CongMotCua_VanHanh];
GO
-- Tiep theo: 01_Tao_13_bang.sql chi chay tren database chua co bang.
-- Ung dung va scripts/setup.ps1 co the tu khoi tao bang cung du lieu mau.
