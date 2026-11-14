param([string]$SqlBackupDirectory='')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$configPath=Join-Path $projectRoot 'publish\appsettings.Local.json'
if(-not(Test-Path -LiteralPath $configPath)){$configPath=Join-Path $projectRoot 'appsettings.Local.json'}
$config=Get-Content -LiteralPath $configPath -Raw -Encoding UTF8|ConvertFrom-Json
$builder=New-Object System.Data.SqlClient.SqlConnectionStringBuilder $config.ConnectionStrings.Default
$database=$builder.InitialCatalog
if($database -notmatch '^[A-Za-z][A-Za-z0-9_]{0,100}$'){throw 'Ten database khong hop le.'}
$connection=New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
$stamp=Get-Date -Format 'yyyyMMdd_HHmmss_fff'
$destination=Join-Path $projectRoot ('.data\backups\'+$stamp)
[void][IO.Directory]::CreateDirectory($destination)
try{
 $connection.Open()
 if(-not $SqlBackupDirectory){$cmd=$connection.CreateCommand();$cmd.CommandText="SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))";$SqlBackupDirectory=[string]$cmd.ExecuteScalar()}
 if(-not $SqlBackupDirectory){throw 'Can truyen -SqlBackupDirectory la thu muc tren may SQL Server.'}
 $serverFile=Join-Path $SqlBackupDirectory ($database+'_'+$stamp+'.bak')
 $cmd=$connection.CreateCommand();$cmd.CommandTimeout=300;$cmd.CommandText="BACKUP DATABASE [$database] TO DISK=@path WITH COPY_ONLY,CHECKSUM";[void]$cmd.Parameters.AddWithValue('@path',$serverFile);[void]$cmd.ExecuteNonQuery()
 $cmd=$connection.CreateCommand();$cmd.CommandTimeout=300;$cmd.CommandText='RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM';[void]$cmd.Parameters.AddWithValue('@path',$serverFile);[void]$cmd.ExecuteNonQuery()
 $localCopy=$false
 if(Test-Path -LiteralPath $serverFile -ErrorAction SilentlyContinue){try{Copy-Item -LiteralPath $serverFile -Destination $destination;$localCopy=$true}catch{Write-Warning "Khong chep duoc .bak ve thu muc du an."}}
 if(-not $localCopy){Write-Warning "SQL backup da verify tai $serverFile. Can DBA chep file .bak tu may SQL Server; thu muc duoi day van chua tep/identity/cau hinh."}
 foreach($item in @(@{source=$config.Files.Root;name='files'},@{source=$config.Identity.Root;name='identity'})){if(Test-Path -LiteralPath $item.source){Copy-Item -LiteralPath $item.source -Destination (Join-Path $destination $item.name) -Recurse}}
 Copy-Item -LiteralPath $configPath -Destination (Join-Path $destination 'appsettings.Local.json')
 @{database=$database;server=$builder.DataSource;time=(Get-Date).ToString('o');verified=$true;localDatabaseCopy=$localCopy;serverBackup=$serverFile}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $destination 'backup-info.json') -Encoding UTF8
 Write-Host "Da sao luu va RESTORE VERIFYONLY: $destination"
}finally{$connection.Dispose()}
