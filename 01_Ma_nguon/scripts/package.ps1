param([string]$Output='')
$ErrorActionPreference='Stop'
$projectRoot=(Resolve-Path -LiteralPath (Split-Path $PSScriptRoot -Parent)).ProviderPath
$root=Split-Path $projectRoot -Parent
$sqlFile=(Resolve-Path -LiteralPath (Join-Path $root '02_Co_so_du_lieu_SQL\CongMotCua_HoanChinh.sql')).ProviderPath
if(-not $Output){$Output=Join-Path $root 'CongMotCua_WebApp.zip'}
$Output=[IO.Path]::GetFullPath($Output)
if(Test-Path -LiteralPath $Output){throw "File dong goi da ton tai, hay dung ten moi: $Output"}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream=[IO.File]::Open($Output,[IO.FileMode]::CreateNew)
$zip=New-Object System.IO.Compression.ZipArchive($stream,[IO.Compression.ZipArchiveMode]::Create,$false)
$files=New-Object System.Collections.Generic.List[System.IO.FileInfo]
try{
 foreach($folder in @('Api','Services','Properties','wwwroot','scripts','keycloak')){
  foreach($file in Get-ChildItem -LiteralPath (Join-Path $projectRoot $folder) -Recurse -File){
   $relative=$file.FullName.Substring($projectRoot.Length+1)
   if($relative -match '(^|[\\/])(bin|obj|node_modules|tests|\.data|\.runtime|\.backups)([\\/]|$)'){continue}
   if($file.Name -eq 'appsettings.Local.json'){continue}
   $files.Add($file)
  }
 }
 foreach($file in Get-ChildItem -LiteralPath $projectRoot -File -Force){if($file.Extension -in @('.cmd','.csproj','.cs','.sln') -or $file.Name -in @('README.md','.gitignore','appsettings.json')){$files.Add($file)}}
 foreach($file in $files){$relative=$file.FullName.Substring($projectRoot.Length+1).Replace('\','/');[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$file.FullName,('CongMotCua_WebApp/01_Ma_nguon/'+$relative),[IO.Compression.CompressionLevel]::Optimal)}
 [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$sqlFile,'CongMotCua_WebApp/02_Co_so_du_lieu_SQL/CongMotCua_HoanChinh.sql',[IO.Compression.CompressionLevel]::Optimal)
}finally{$zip.Dispose();$stream.Dispose()}
Write-Host "Da dong goi $($files.Count) file: $Output"
Write-Host 'Kem tep SQL khoi tao; khong dong goi database dang dung, danh tinh hay tep nguoi dung.'
