<#
.SYNOPSIS
  Clovance updater for Windows.
.DESCRIPTION
  Thin wrapper around install.ps1's "update" command so the documented
  one-liner works:

    irm https://evaristocuesta.github.io/clovance/install/update.ps1 | iex

  It uses the install.ps1 sitting next to this file when running from a
  checkout, and downloads the published one otherwise.
#>

$ErrorActionPreference = "Stop"
$InstallerUrl = "https://evaristocuesta.github.io/clovance/install/install.ps1"

$source = $null
if ($PSScriptRoot) {
  $local = Join-Path $PSScriptRoot "install.ps1"
  if (Test-Path $local) { $source = Get-Content $local -Raw }
}
if (-not $source) {
  Write-Host "==> Downloading installer..." -ForegroundColor Cyan
  $source = Invoke-RestMethod $InstallerUrl
}

# "& { ... } update" binds the command to install.ps1's $Command parameter.
& ([scriptblock]::Create($source)) update
