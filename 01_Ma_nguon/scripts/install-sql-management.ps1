param([string]$Server='localhost',[string]$Database='CongMotCua_VanHanh')
$ErrorActionPreference='Stop'
if($Database -notmatch '^[A-Za-z][A-Za-z0-9_]{0,100}$'){throw 'Ten database khong hop le'}
$projectRoot=Split-Path $PSScriptRoot -Parent
$script=Get-Content -LiteralPath (Join-Path $projectRoot 'sql\02_Quan_tri_va_rang_buoc.sql') -Raw -Encoding UTF8
$script=$script.Replace('[CongMotCua_VanHanh]',('['+$Database+']'))
$connection=New-Object System.Data.SqlClient.SqlConnection ('Server='+$Server+';Database='+$Database+';Integrated Security=True;TrustServerCertificate=True')
try{$connection.Open();foreach($batch in [regex]::Split($script,'(?im)^GO\s*\r?$')){if(-not [string]::IsNullOrWhiteSpace($batch)){$cmd=$connection.CreateCommand();$cmd.CommandText=$batch;$cmd.CommandTimeout=60;[void]$cmd.ExecuteNonQuery()}}}finally{$connection.Dispose()}
Write-Host 'Da cai views, stored procedures, triggers va roles quan tri SQL Server.'
