param([string]$Server='localhost',[string]$Database='CongMotCua_VanHanh')
$ErrorActionPreference='Stop'
if($Database -notmatch '^[A-Za-z][A-Za-z0-9_]{0,100}$'){throw 'Ten database khong hop le'}
$projectRoot=Split-Path $PSScriptRoot -Parent
$sqlPath=Join-Path (Split-Path $projectRoot -Parent) '02_Co_so_du_lieu_SQL\CongMotCua_HoanChinh.sql'
$script=Get-Content -LiteralPath $sqlPath -Raw -Encoding UTF8
$section=[regex]::Match($script,'(?ms)^[ \t]*-- BEGIN QUAN_TRI\r?\n(?<sql>.*?)^[ \t]*-- END QUAN_TRI[ \t]*\r?$')
if(-not $section.Success){throw 'Khong tim thay phan quan tri trong CongMotCua_HoanChinh.sql.'}
$connection=New-Object System.Data.SqlClient.SqlConnection ('Server='+$Server+';Database='+$Database+';Integrated Security=True;TrustServerCertificate=True')
try {
 $connection.Open()
 $command=$connection.CreateCommand()
 try {
  $command.CommandText="SET XACT_ABORT ON; BEGIN TRY BEGIN TRANSACTION;`n" + $section.Groups['sql'].Value + "`nCOMMIT TRANSACTION; END TRY BEGIN CATCH IF XACT_STATE() <> 0 ROLLBACK TRANSACTION; THROW; END CATCH;"
  $command.CommandTimeout=60
  [void]$command.ExecuteNonQuery()
 } finally { $command.Dispose() }
} finally { $connection.Dispose() }
Write-Host 'Da cai views, stored procedures, triggers va roles quan tri SQL Server.'
