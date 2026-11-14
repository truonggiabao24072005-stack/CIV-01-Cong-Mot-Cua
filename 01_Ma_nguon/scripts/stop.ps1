$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$pidFile=Join-Path $projectRoot '.data\server.pid'
if(-not(Test-Path -LiteralPath $pidFile)){Write-Host 'Khong co PID do script khoi dong luu.';exit 0}
$appProcessId=[int](Get-Content -LiteralPath $pidFile)
$process=Get-CimInstance Win32_Process -Filter "ProcessId=$appProcessId"
$expected=Join-Path $projectRoot 'publish\CongMotCua.dll'
if($process -and $process.CommandLine -and $process.CommandLine.Contains($expected)){Stop-Process -Id $appProcessId;Write-Host 'Da dung ung dung.'}elseif($process){throw 'PID da thuoc chuong trinh khac; khong dung tien trinh.'}
Remove-Item -LiteralPath $pidFile -ErrorAction SilentlyContinue
