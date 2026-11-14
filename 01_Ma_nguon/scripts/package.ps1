param([string]$Output='')
$ErrorActionPreference='Stop'
$projectRoot=(Resolve-Path -LiteralPath (Split-Path $PSScriptRoot -Parent)).ProviderPath
if(-not $Output){$Output=Join-Path (Split-Path $projectRoot -Parent) 'CongMotCua_WebApp.zip'}
$Output=[IO.Path]::GetFullPath($Output)
if(Test-Path -LiteralPath $Output){throw "File dong goi da ton tai, hay dung ten moi: $Output"}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream=[IO.File]::Open($Output,[IO.FileMode]::CreateNew)
$zip=New-Object System.IO.Compression.ZipArchive($stream,[IO.Compression.ZipArchiveMode]::Create,$false)
$files=New-Object System.Collections.Generic.List[System.IO.FileInfo]
try{
 foreach($folder in @('Api','Services','Properties','wwwroot','sql','scripts','docs','keycloak')){
  foreach($file in Get-ChildItem -LiteralPath (Join-Path $projectRoot $folder) -Recurse -File){
   $relative=$file.FullName.Substring($projectRoot.Length+1)
   if($relative -match '(^|[\\/])(bin|obj|node_modules|tests|\.data|\.runtime|\.backups)([\\/]|$)'){continue}
   if($file.Name -eq 'appsettings.Local.json'){continue}
   $files.Add($file)
  }
 }
 foreach($file in Get-ChildItem -LiteralPath $projectRoot -File -Force){if($file.Extension -in @('.cmd','.csproj','.cs','.sln') -or $file.Name -in @('README.md','.gitignore','appsettings.json')){$files.Add($file)}}
 foreach($file in $files){$relative=$file.FullName.Substring($projectRoot.Length+1).Replace('\','/');[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$file.FullName,('CongMotCua_WebApp/'+$relative),[IO.Compression.CompressionLevel]::Optimal)}
}finally{$zip.Dispose();$stream.Dispose()}
Write-Host "Da dong goi $($files.Count) file: $Output"
Write-Host 'Khong dong goi database dang dung, danh tinh/khóa ky, tep nguoi dung, cache va file test. Du lieu mau co trong bo SQL.'
