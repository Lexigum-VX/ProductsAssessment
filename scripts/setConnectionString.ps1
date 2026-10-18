
param(
    [Parameter(Mandatory = $true)]
    [string]$Password
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$apiPath = Join-Path $root "src/Products.Api"

$connectionString = "Server=127.0.0.1,14333;" +
    "Database=ProductsDb;" +
    "User Id=sa;" +
    "Password=$Password;" +
    "Encrypt=True;" +
    "TrustServerCertificate=True;"

Push-Location $apiPath

try {
    dotnet user-secrets init

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to initialize User Secrets."
    }

    dotnet user-secrets set `
        "ConnectionStrings:ProductsDb" `
        $connectionString

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to configure connection string."
    }

    Write-Host "Connection string configured."
}
finally {
    Pop-Location
}
