$pgUser = "postgres"
$pgPassword = Read-Host "PostgreSQL Password" -AsSecureString
$pgHost = "localhost"
$pgPort = "5432"

$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pgPassword))
$env:PGUSER = $pgUser
$env:PGHOST = $pgHost
$env:PGPORT = $pgPort

rm R:\pg_ram_db -Recurse
psql --csv -d postgres -c "DROP TABLESPACE ram_tablespace;"

Remove-Item -Path "R:\pg_ram_db" -Recurse

Remove-Item Env:PGPASSWORD
Remove-Item Env:PGUSER
Remove-Item Env:PGHOST
Remove-Item Env:PGPORT

Pause