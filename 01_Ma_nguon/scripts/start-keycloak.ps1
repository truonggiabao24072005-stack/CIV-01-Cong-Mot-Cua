$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$taskKc=Join-Path $taskRoot '.runtime\keycloak-26.8.1'
$taskJava=Get-ChildItem -LiteralPath (Join-Path $taskRoot '.runtime\java21') -Directory -ErrorAction SilentlyContinue | Select-Object -First 1
if ($taskJava) { $env:JAVA_HOME=$taskJava.FullName }
$env:JAVA_OPTS_APPEND='-Djava.net.preferIPv4Stack=true -Djava.net.preferIPv4Addresses=true -XX:ActiveProcessorCount=4 -Xmx768m'
if (-not (Test-Path (Join-Path $taskKc 'bin\kc.bat'))) { & (Join-Path $PSScriptRoot 'download-keycloak.ps1') }
$taskImport=Join-Path $taskKc 'data\import'
New-Item -ItemType Directory -Force -Path $taskImport | Out-Null
Copy-Item -LiteralPath (Join-Path $taskRoot 'keycloak\congmotcua-realm.json') -Destination (Join-Path $taskImport 'congmotcua-realm.json') -Force
& (Join-Path $taskKc 'bin\kc.bat') start-dev --http-host=127.0.0.1 --http-port=8180 --hostname=http://localhost:8180 --import-realm
if ($LASTEXITCODE -ne 0) { throw 'Keycloak start failed' }
