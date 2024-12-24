#!/usr/bin/env pwsh

param([String]$name)

if ($name -eq "")
{
    Write-Error "must specify migration name"
    exit
}

dotnet ef migrations add --context SqliteServerDbContext --output-dir Migrations/Sqlite $name
dotnet ef migrations add --context PostgresServerDbContext --output-dir Migrations/Postgres $name
