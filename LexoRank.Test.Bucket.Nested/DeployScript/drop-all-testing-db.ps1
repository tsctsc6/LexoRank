# Set PostgreSQL connection parms
$pgUser = "postgres"             # PostgreSQL username
$pgPassword = Read-Host "PostgreSQL Password" -AsSecureString    # PostgreSQL password
$pgHost = "localhost"
$pgPort = "5432"
$prefix = "lexorank_test_"  # database prefix

$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pgPassword))
$env:PGUSER = $pgUser
$env:PGHOST = $pgHost
$env:PGPORT = $pgPort

$queryString = @"
SELECT row_to_json(t) FROM (
SELECT datname FROM pg_database
WHERE
datname LIKE '$prefix%' AND
datname NOT IN ('postgres', 'template0', 'template1')
) t;
"@

# Retrieve the database list that matches the prefix(connected to database 'postgres')
$dbs = & psql -d postgres -Atc $queryString

# no database matches
if (-not $dbs) {
    Write-Host "No databases starting with '$prefix' were found."
    Pause
    exit
}

$dbs = $dbs.Split("`r`n");

foreach ($db in $dbs) {
    $db = $db | ConvertFrom-Json
    Write-Host "process database: $($db.datname)"

    $disconnectString = @"
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity WHERE datname = '$($db.datname)'
AND pid <> pg_backend_pid();
"@

    # Terminate all existing connections to the database (except for the current session).
    & psql -d postgres -c $disconnectString > $null

    # delete database
    & dropdb --if-exists $($db.datname)
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Delete database successfully: $($db.datname)"
    }
    else {
        Write-Host "Delete database failed: $($db.datname)"
    }
}

# clear env var(optional)
Remove-Item Env:PGPASSWORD
Remove-Item Env:PGUSER
Remove-Item Env:PGHOST
Remove-Item Env:PGPORT

Write-Host "Done"
Pause