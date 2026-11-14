param(
 [Parameter(Mandatory)][string]$TenDangNhap,
 [Parameter(Mandatory)][string]$HoTen,
 [Parameter(Mandatory)][string]$Email,
 [Parameter(Mandatory)][ValidateSet('TIEP_NHAN','GIAI_QUYET')][string]$VaiTro,
 [string]$MaBoPhan
)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$publish=Join-Path $project 'publish'
$dll=Join-Path $publish 'CongMotCua.dll'
if(!(Test-Path -LiteralPath $dll)){throw 'Chay scripts\setup.ps1 truoc khi cap tai khoan.'}
$runningPidFile=Join-Path $project '.data\server.pid'
if(Test-Path -LiteralPath $runningPidFile){
 $runningPid=[int](Get-Content -LiteralPath $runningPidFile)
 if(Get-Process -Id $runningPid -ErrorAction SilentlyContinue){throw 'Dung web bang scripts\stop.ps1 truoc khi cap tai khoan.'}
}
$commandArgs=@('--cap-tai-khoan',$TenDangNhap,$HoTen,$Email,$VaiTro)
if($MaBoPhan){$commandArgs+=$MaBoPhan}
Push-Location -LiteralPath $publish
try{
 & dotnet $dll @commandArgs
 if($LASTEXITCODE -ne 0){throw 'Khong cap duoc tai khoan.'}
}finally{Pop-Location}
