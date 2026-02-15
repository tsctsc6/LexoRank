$pgUser = "postgres"
$pgPassword = Read-Host "PostgreSQL Password" -AsSecureString
$pgHost = "localhost"
$pgPort = "5432"

$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pgPassword))
$env:PGUSER = $pgUser
$env:PGHOST = $pgHost
$env:PGPORT = $pgPort

mkdir R:\pg_ram_db
psql --csv -d postgres -c "CREATE TABLESPACE ram_tablespace LOCATION 'R:/pg_ram_db';"

Remove-Item Env:PGPASSWORD
Remove-Item Env:PGUSER
Remove-Item Env:PGHOST
Remove-Item Env:PGPORT

Pause