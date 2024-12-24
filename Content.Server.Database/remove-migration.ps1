#!/usr/bin/env pwsh

param([String]$name)

if ($name -eq "")
{
    Write-Error "must specify migration name"
    exit
}

dotnet ef migrations remove --context SqliteServerDbContext $name
dotnet ef migrations remove --context PostgresServerDbContext $name
