param([string]$Server='localhost',[string]$Database='CongMotCua_VanHanh',[switch]$Build,[switch]$NapDuLieuMau)
$ErrorActionPreference='Stop'
if($Database -notmatch '^[A-Za-z][A-Za-z0-9_]{0,100}$'){throw 'Ten database khong hop le.'}
$projectRoot=Split-Path $PSScriptRoot -Parent
$publish=Join-Path $projectRoot 'publish'
if(-not(Get-Command dotnet -ErrorAction SilentlyContinue)){throw 'Can cai .NET 9 ASP.NET Core Runtime (ban dong goi) hoac SDK 9 (bien dich ma nguon).'}
$localConfigPath=Join-Path $projectRoot 'appsettings.Local.json'
$dataRoot=Join-Path $projectRoot '.data'
$operationRoot=Join-Path $dataRoot ('van-hanh\'+$Database)
$config=@{ConnectionStrings=@{Default="Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=False"};Auth=@{Mode='Local'};Files=@{Root=(Join-Path $operationRoot 'files')};Identity=@{Root=(Join-Path $operationRoot 'identity')}}
$reuseConfig=(Test-Path -LiteralPath $localConfigPath) -and -not $NapDuLieuMau -and -not $PSBoundParameters.ContainsKey('Database') -and -not $PSBoundParameters.ContainsKey('Server')
if($reuseConfig){
 $config=Get-Content -LiteralPath $localConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
 $connection=[System.Data.SqlClient.SqlConnectionStringBuilder]::new([string]$config.ConnectionStrings.Default)
 $Server=$connection.DataSource
 $Database=$connection.InitialCatalog
 if($Database -notmatch '^[A-Za-z][A-Za-z0-9_]{0,100}$' -or -not $Server){throw 'Kiem tra may chu va ten database trong appsettings.Local.json.'}
}
if($NapDuLieuMau){
 if(-not $PSBoundParameters.ContainsKey('Database') -or $Database -in @('CongMotCua_VanHanh','CongMotCua_DoAn13_Release')){throw 'Chi dinh database kiem thu rieng bang -Database khi nap du lieu mau.'}
 $sampleRoot=Join-Path $dataRoot ('kiem-thu\'+$Database)
 if(Test-Path -LiteralPath $sampleRoot){throw 'Kho du lieu mau da ton tai. Dung ten database kiem thu moi hoac chay lai web voi cau hinh hien co.'}
 $passwordBytes=New-Object byte[] 24
 $random=[System.Security.Cryptography.RandomNumberGenerator]::Create()
 try{$random.GetBytes($passwordBytes)}finally{$random.Dispose()}
 $config.Identity=@{Root=(Join-Path $sampleRoot 'identity');SamplePassword=[Convert]::ToBase64String($passwordBytes)}
 $config.Files.Root=Join-Path $sampleRoot 'files'
}
if($Build -or -not(Test-Path -LiteralPath (Join-Path $publish 'CongMotCua.dll'))){
 & dotnet publish (Join-Path $projectRoot 'CongMotCua.csproj') -c Release -o $publish --nologo
 if($LASTEXITCODE -ne 0){throw 'Publish API that bai'}
}
$config|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $publish 'appsettings.Local.json') -Encoding UTF8
Push-Location $publish
try{
 $runArgs=@('--initialize')
 if($NapDuLieuMau){$runArgs+='--seed-sample'}
 & dotnet (Join-Path $publish 'CongMotCua.dll') @runArgs
 if($LASTEXITCODE -ne 0){throw 'Khoi tao SQL Server that bai. Kiem tra instance va quyen CREATE DATABASE.'}
}finally{Pop-Location}
& (Join-Path $PSScriptRoot 'install-sql-management.ps1') -Server $Server -Database $Database
if(-not $reuseConfig){$config|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $localConfigPath -Encoding UTF8}
Write-Host "Hoan tat. SQL Server: $Server / $Database. Chay scripts\start.ps1."

if($NapDuLieuMau){Write-Host 'Mat khau kiem thu nam trong Identity.SamplePassword cua appsettings.Local.json.'}
else{Write-Host 'Cap tai khoan can bo bang scripts\cap-tai-khoan.ps1 truoc khi mo web.'}
