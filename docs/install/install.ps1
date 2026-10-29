<#
.SYNOPSIS
  Clovance installer for Windows.
.DESCRIPTION
  Usage:
    .\install.ps1            (install)
    .\install.ps1 install
    .\install.ps1 uninstall
    .\install.ps1 update
#>
param(
  [ValidateSet("install","uninstall","update")]
  [string]$Command = "install"
)

$ErrorActionPreference = "Stop"

$DefaultInstallDir  = Join-Path $env:USERPROFILE "clovance"
$DefaultVersion     = "latest"
$DefaultFrontendPort = "7000"
$DefaultApiPort     = "8080"
$DefaultBind        = "0.0.0.0"
$DefaultDashBind    = "127.0.0.1"

function Log   { param($m) Write-Host "==> $m" -ForegroundColor Cyan }
function Warn  { param($m) Write-Host "!!  $m" -ForegroundColor Yellow }
function Err   { param($m) Write-Host "xx  $m" -ForegroundColor Red }

function Prompt {
  param([string]$Message, [string]$Default)
  if ([Environment]::UserInteractive) {
    $val = Read-Host "$Message [$Default]"
    if ([string]::IsNullOrWhiteSpace($val)) { $val = $Default }
  } else { $val = $Default }
  return $val
}

function PromptSecret {
  param([string]$Message)
  if ([Environment]::UserInteractive) {
    $sec = Read-Host $Message -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
  }
  return ""
}

function GenPassword {
  $bytes = New-Object byte[] 18
  [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
  return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','_').Replace('/','-')
}

function Require($cmd) {
  if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) { Err "Missing required command: $cmd"; exit 1 }
}

function Check-Docker {
  Require docker
  try { docker info 2>$null | Out-Null } catch { Err "Docker daemon is not running. Start Docker and retry."; exit 1 }
  $v = docker compose version 2>$null
  if (-not $v) { Err "Docker Compose plugin (v2) is required."; exit 1 }
}

function ToPosixPath($p) {
  # Convert Windows path to Docker-friendly POSIX path (C:\foo -> /c/foo)
  $p = $p -replace '\\','/'
  if ($p -match '^([A-Za-z]):/(.*)$') {
    $drive = $matches[1].ToLower()
    $rest  = $matches[2]
    return "/$drive/$rest"
  }
  return $p
}

$ComposeContent = @'
services:
  env-dashboard:
    image: "mcr.microsoft.com/dotnet/nightly/aspire-dashboard:13.6"
    ports:
      - "${CLOVANCE_DASHBOARD_BIND_ADDRESS:-0.0.0.0}::18888"
    expose:
      - "18889"
      - "18890"
    networks:
      - "aspire"
    restart: "always"
  clovance-postgres:
    image: "docker.io/library/postgres:18.3"
    environment:
      POSTGRES_HOST_AUTH_METHOD: "scram-sha-256"
      POSTGRES_INITDB_ARGS: "--auth-host=scram-sha-256 --auth-local=scram-sha-256"
      POSTGRES_USER: "${POSTGRES_USERNAME}"
      POSTGRES_PASSWORD: "${POSTGRES_PASSWORD}"
      POSTGRES_DB: "clovance-database"
    expose:
      - "5432"
    volumes:
      - type: "bind"
        target: "/var/lib/postgresql"
        source: "${VOLUME_SOURCE_PATH}/postgres"
        read_only: false
    networks:
      - "aspire"
    restart: "unless-stopped"
  clovance-apiservice:
    image: "${CLOVANCE_APISERVICE_IMAGE}"
    environment:
      OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY: "in_memory"
      ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"
      HTTP_PORTS: "${CLOVANCE_APISERVICE_PORT}"
      ConnectionStrings__clovance-database: "Host=clovance-postgres;Port=5432;Username=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD};Database=clovance-database"
      CLOVANCE_DATABASE_HOST: "clovance-postgres"
      CLOVANCE_DATABASE_PORT: "5432"
      CLOVANCE_DATABASE_USERNAME: "${POSTGRES_USERNAME}"
      CLOVANCE_DATABASE_PASSWORD: "${POSTGRES_PASSWORD}"
      CLOVANCE_DATABASE_URI: "postgresql://${POSTGRES_USERNAME}:${POSTGRES_PASSWORD}@clovance-postgres:5432/clovance-database"
      CLOVANCE_DATABASE_JDBCCONNECTIONSTRING: "jdbc:postgresql://clovance-postgres:5432/clovance-database"
      CLOVANCE_DATABASE_DATABASENAME: "clovance-database"
      ASPNETCORE_ENVIRONMENT: "Production"
      Jwt__KeyFilePath: "/home/app/jwt.key"
      Smtp__Host: "${SMTP_HOST}"
      Smtp__Port: "${SMTP_PORT}"
      Smtp__Username: "${SMTP_USERNAME}"
      Smtp__Password: "${SMTP_PASSWORD}"
      Smtp__FromAddress: "${SMTP_FROM_ADDRESS}"
      Smtp__FromName: "${SMTP_FROM_NAME}"
      Frontend__BaseUrl: "${FRONTEND_BASE_URL}"
      OTEL_EXPORTER_OTLP_ENDPOINT: "http://env-dashboard:18889"
      OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"
      OTEL_SERVICE_NAME: "clovance-apiservice"
    expose:
      - "${CLOVANCE_APISERVICE_PORT}"
    volumes:
      - type: "bind"
        target: "/home/app"
        source: "${VOLUME_SOURCE_PATH}/jwt"
        read_only: false
    depends_on:
      clovance-postgres:
        condition: "service_started"
    networks:
      - "aspire"
    restart: "unless-stopped"
  clovance-frontend:
    image: "${CLOVANCE_FRONTEND_IMAGE}"
    command:
      - "nginx"
      - "-g"
      - "daemon off;"
    entrypoint:
      - "/docker-entrypoint.sh"
    environment:
      NODE_ENV: "production"
      CLOVANCE_APISERVICE_HTTP: "http://clovance-apiservice:${CLOVANCE_APISERVICE_PORT}"
      services__clovance-apiservice__http__0: "http://clovance-apiservice:${CLOVANCE_APISERVICE_PORT}"
      CLOVANCE_APISERVICE_HTTPS: "https://clovance-apiservice:${CLOVANCE_APISERVICE_PORT}"
      PORT: "7000"
      OTEL_EXPORTER_OTLP_ENDPOINT: "http://env-dashboard:18889"
      OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"
      OTEL_SERVICE_NAME: "clovance-frontend"
    ports:
      - "${CLOVANCE_BIND_ADDRESS:-0.0.0.0}:7000:7000"
    depends_on:
      clovance-apiservice:
        condition: "service_started"
    networks:
      - "aspire"
    restart: "unless-stopped"
networks:
  aspire:
    driver: "bridge"
'@

function Do-Install {
  Check-Docker
  $installDir = Prompt "Install directory" $DefaultInstallDir
  $version    = Prompt "Image version tag (e.g. latest, v1.0.0)" $DefaultVersion
  $fePort     = Prompt "Frontend host port" $DefaultFrontendPort
  $apiPort    = Prompt "API container port" $DefaultApiPort
  $bind       = Prompt "Bind address for frontend" $DefaultBind
  $dashBind   = Prompt "Bind address for Aspire dashboard" $DefaultDashBind
  $pgUser     = Prompt "PostgreSQL username" "clovance"
  $pgPass     = PromptSecret "PostgreSQL password (empty = auto-generate)"
  if ([string]::IsNullOrWhiteSpace($pgPass)) { $pgPass = GenPassword; Log "Generated PostgreSQL password." }
  $smtpHost   = Prompt "SMTP host" "smtp.example.com"
  $smtpPort   = Prompt "SMTP port" "587"
  $smtpUser   = Prompt "SMTP username" "your_smtp_username"
  $smtpPass   = PromptSecret "SMTP password"
  $smtpFrom   = Prompt "SMTP from address" "your_smtp_from_address@example.com"
  $smtpName   = Prompt "SMTP from name" "Clovance"
  $feUrl      = Prompt "Frontend base URL" "http://localhost:$fePort"

  New-Item -ItemType Directory -Force -Path $installDir | Out-Null
  $volumePath = Join-Path $installDir "data"
  New-Item -ItemType Directory -Force -Path (Join-Path $volumePath "postgres") | Out-Null
  New-Item -ItemType Directory -Force -Path (Join-Path $volumePath "jwt") | Out-Null

  $volumePosix = ToPosixPath $volumePath
  $apiImage = "ghcr.io/evaristocuesta/clovance/clovance-api:$version"
  $feImage  = "ghcr.io/evaristocuesta/clovance/clovance-frontend:$version"

  $envContent = @"
CLOVANCE_APISERVICE_IMAGE=$apiImage
CLOVANCE_APISERVICE_PORT=$apiPort
CLOVANCE_FRONTEND_IMAGE=$feImage
FRONTEND_BASE_URL=$feUrl
POSTGRES_PASSWORD=$pgPass
POSTGRES_USERNAME=$pgUser
SMTP_FROM_ADDRESS=$smtpFrom
SMTP_FROM_NAME=$smtpName
SMTP_HOST=$smtpHost
SMTP_PASSWORD=$smtpPass
SMTP_PORT=$smtpPort
SMTP_USERNAME=$smtpUser
VOLUME_SOURCE_PATH=$volumePosix
CLOVANCE_BIND_ADDRESS=$bind
CLOVANCE_DASHBOARD_BIND_ADDRESS=$dashBind
"@
  Set-Content -Path (Join-Path $installDir ".env") -Value $envContent -NoNewline
  Set-Content -Path (Join-Path $installDir "docker-compose.yml") -Value $ComposeContent -NoNewline

  Log "Configuration written to $installDir"

  Push-Location $installDir
  try {
    Log "Pulling images..."
    docker compose pull
    Log "Starting Clovance..."
    docker compose up -d
  } finally {
    Pop-Location
  }

  Write-Host ""
  Log "Clovance is running!"
  Write-Host "   Frontend:  http://localhost:$fePort"
  Write-Host "   Data dir:  $volumePath"
  Write-Host ""
  Write-Host "   Aspire dashboard token:"
  Write-Host "     docker logs (docker compose ps -q env-dashboard) 2>&1 | Select-String token"
  Write-Host ""
  Write-Host "   Stop:    cd $installDir; docker compose down"
  Write-Host "   Update:  .\install.ps1 update"
}

function Do-Uninstall {
  Check-Docker
  $installDir = Prompt "Install directory" $DefaultInstallDir
  $compose = Join-Path $installDir "docker-compose.yml"
  if (-not (Test-Path $compose)) { Err "No docker-compose.yml found in $installDir"; exit 1 }
  Push-Location $installDir
  try {
    Log "Stopping containers..."
    docker compose down
  } finally { Pop-Location }
  $rmvol = Prompt "Remove data volumes (postgres + jwt)? (y/N)" "N"
  if ($rmvol -match '^[Yy]$') {
    Remove-Item -Recurse -Force (Join-Path $installDir "data") -ErrorAction SilentlyContinue
    Log "Data removed."
  }
  $rmcfg = Prompt "Remove generated config files (.env, docker-compose.yml)? (y/N)" "N"
  if ($rmcfg -match '^[Yy]$') {
    Remove-Item -Force (Join-Path $installDir ".env") -ErrorAction SilentlyContinue
    Remove-Item -Force $compose -ErrorAction SilentlyContinue
    Log "Config removed."
  }
  Log "Uninstall complete."
}

function Do-Update {
  Check-Docker
  $installDir = Prompt "Install directory" $DefaultInstallDir
  $envPath = Join-Path $installDir ".env"
  if (-not (Test-Path $envPath)) { Err "No .env found in $installDir"; exit 1 }
  $version = Prompt "New image version tag" $DefaultVersion
  Copy-Item $envPath "$envPath.bak" -Force
  $lines = Get-Content $envPath
  $lines = $lines | ForEach-Object {
    if ($_ -match '^CLOVANCE_APISERVICE_IMAGE=') { "CLOVANCE_APISERVICE_IMAGE=ghcr.io/evaristocuesta/clovance/clovance-api:$version" }
    elseif ($_ -match '^CLOVANCE_FRONTEND_IMAGE=') { "CLOVANCE_FRONTEND_IMAGE=ghcr.io/evaristocuesta/clovance/clovance-frontend:$version" }
    else { $_ }
  }
  Set-Content -Path $envPath -Value ($lines -join "`r`n") -NoNewline
  Push-Location $installDir
  try {
    Log "Pulling new images..."
    docker compose pull
    Log "Recreating containers..."
    docker compose up -d
  } finally { Pop-Location }
  Log "Update complete."
}

switch ($Command) {
  "install"   { Do-Install }
  "uninstall" { Do-Uninstall }
  "update"    { Do-Update }
}