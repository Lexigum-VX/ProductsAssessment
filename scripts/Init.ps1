param(
    [switch]$RunTests
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

$dockerPath = Join-Path $root "docker"
$composeFile = Join-Path $dockerPath "compose.yaml"
$envFile = Join-Path $dockerPath ".env"
$envExample = Join-Path $dockerPath ".env.example"

$apiPath = Join-Path $root "src/Products.Api"
$infraPath = Join-Path $root "src/Products.Infrastructure"

function Assert-Success {
    param([string]$Message)

    if ($LASTEXITCODE -ne 0) {
        throw $Message
    }
}

Write-Host "Initializing Products Assessment..."

# 1. Verify prerequisites
dotnet --version
Assert-Success ".NET SDK is required."

docker info --format '{{.ServerVersion}}' | Out-Null
Assert-Success "Docker must be installed and running."

dotnet ef --version
Assert-Success "EF Core CLI is required."

# 2. Create local environment file if missing
if (-not (Test-Path $envFile)) {
    Copy-Item $envExample $envFile

    Write-Host "Created docker/.env"
    Write-Host "Configure SQL_SA_PASSWORD in docker/.env and run Init.ps1 again."
    exit 0
}

# 3. Load local SQL password
$passwordLine = Get-Content $envFile |
    Where-Object { $_ -match '^SQL_SA_PASSWORD=' } |
    Select-Object -First 1

if (-not $passwordLine) {
    throw "SQL_SA_PASSWORD is missing from docker/.env."
}

$password = ($passwordLine -split '=', 2)[1]

if (
    [string]::IsNullOrWhiteSpace($password) -or
    $password -eq "REPLACE_WITH_STRONG_LOCAL_PASSWORD"
) {
    throw "Configure SQL_SA_PASSWORD in docker/.env."
}

if ($password -match '[;"\r\n]') {
    throw "The local password cannot contain semicolons or quotation marks."
}

# 4. Start SQL Server
Write-Host "Starting SQL Server..."

docker compose `
    --env-file $envFile `
    -f $composeFile `
    up -d

Assert-Success "Failed to start SQL Server."

# 5. Configure the API connection string
& (Join-Path $PSScriptRoot "SetConnectionString.ps1") `
    -Password $password

Write-Host "Building solution..."

dotnet build (Join-Path $root "Products.slnx") --nologo

Assert-Success "Solution build failed. Fix compilation errors before initializing the database."


# 6. Wait for database and apply migrations
Write-Host "Waiting for SQL Server and applying migrations..."

$maxAttempts = 5
$migrationSucceeded = $false

Push-Location $root

try {
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        Write-Host "Migration attempt $attempt/$maxAttempts"

        dotnet ef database update `
            --project $infraPath `
            --startup-project $apiPath

        if ($LASTEXITCODE -eq 0) {
            $migrationSucceeded = $true
            break
        }

        if ($attempt -lt $maxAttempts) {
            Start-Sleep -Seconds 5
        }
    }
}
finally {
    Pop-Location
}

if (-not $migrationSucceeded) {
    throw "Database initialization failed. Check SQL Server logs and configuration."
}

if ($RunTests) {
    Write-Host "Running automated tests..."

    dotnet test (Join-Path $root "Products.slnx")

    if ($LASTEXITCODE -ne 0) {
        throw "Automated tests failed."
    }
}

Write-Host ""
Write-Host "Database initialization completed successfully." -ForegroundColor Green
Write-Host "You can now start Products.Api in Visual Studio."