$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$taskRuntime=Join-Path $taskRoot '.runtime'
New-Item -ItemType Directory -Force -Path $taskRuntime | Out-Null
$taskZip=Join-Path $taskRuntime 'keycloak.zip'
$taskVersion='26.8.1'
if (-not (Test-Path (Join-Path $taskRuntime "keycloak-$taskVersion\bin\kc.bat"))) {
  Write-Output 'Downloading official Keycloak distribution...'
  Invoke-WebRequest -Uri "https://github.com/keycloak/keycloak/releases/download/$taskVersion/keycloak-$taskVersion.zip" -OutFile $taskZip
  Expand-Archive -LiteralPath $taskZip -DestinationPath $taskRuntime -Force
}
Write-Output 'Keycloak ready'
