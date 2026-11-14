param([int]$Port=5080,[switch]$NoBrowser)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$publish=Join-Path $projectRoot 'publish'
$dll=Join-Path $publish 'CongMotCua.dll'
$url="http://localhost:$Port"
try{$status=Invoke-RestMethod "$url/api/health" -TimeoutSec 2;if($status.status -eq 'ok' -and $status.version -eq '2.0.0'){Write-Host "He thong dang chay: $url";if(-not $NoBrowser){Start-Process $url};exit 0}}catch{}
if(-not(Test-Path -LiteralPath (Join-Path $publish 'appsettings.Local.json'))){& (Join-Path $PSScriptRoot 'setup.ps1')}
if(-not(Test-Path -LiteralPath $dll)){throw 'Chua co ban chay. Hay chay scripts\setup.ps1 -Build.'}
$data=Join-Path $projectRoot '.data'
[void][IO.Directory]::CreateDirectory($data)
$stdout=Join-Path $data 'server.log';$stderr=Join-Path $data 'server-error.log'
$process=Start-Process -FilePath 'dotnet' -ArgumentList @(('"'+$dll+'"'),'--urls',$url) -WorkingDirectory $publish -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
$process.Id|Set-Content -LiteralPath (Join-Path $data 'server.pid') -Encoding ASCII
$ok=$false
for($attempt=0;$attempt -lt 30;$attempt++){Start-Sleep -Milliseconds 500;try{if((Invoke-RestMethod "$url/api/health" -TimeoutSec 2).status -eq 'ok'){$ok=$true;break}}catch{};if($process.HasExited){break}}
if(-not $ok){throw "Khong khoi dong duoc. Xem $stderr va $stdout"}
Write-Host "Da khoi dong $url (PID $($process.Id)). SQL va tep duoc luu sau khi tat trinh duyet."
if(-not $NoBrowser){Start-Process $url}
