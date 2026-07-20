#!/usr/bin/env pwsh
# Script to create and publish a new version
# Uso: 
#   .\scripts\publish-version.ps1 -Version "1.0.0"
#   .\scripts\publish-version.ps1 -Version "1.0.0-beta1"
#   .\scripts\publish-version.ps1 -Version "2.0.0-rc.2"

param(
	[Parameter(Mandatory=$true)]
	[string]$Version,

	[switch]$DryRun
)

# Validate version format (full semantic versioning)
# Accepts: X.Y.Z, X.Y.Z-prerelease, X.Y.Z-prerelease.number
if ($Version -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$') {
	Write-Error @"
The version must follow Semantic Versioning:
  - Production: X.Y.Z (e.g., 1.0.0)
  - Pre-release: X.Y.Z-prerelease (e.g., 1.0.0-beta1, 2.0.0-rc.2, 1.0.0-alpha)
"@
	exit 1
}

# Detect if it's a pre-release
$isPreRelease = $Version -match '-'

$tag = "v$Version"

Write-Host "📦 Creating version $tag" -ForegroundColor Cyan

# Verify that we are on the correct branch
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "🌿 Current branch: $currentBranch" -ForegroundColor Yellow

# Verify that there are no uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus) {
	Write-Error "⚠️  There are uncommitted changes. Please commit or discard them before creating a version."
	git status --short
	exit 1
}

# Verify that we are up to date with the remote

Write-Host "📡 Synchronizing with the remote repository..." -ForegroundColor Cyan
git fetch origin

$localCommit = git rev-parse HEAD
$remoteCommit = git rev-parse origin/$currentBranch

if ($localCommit -ne $remoteCommit) {
	Write-Warning "⚠️  Your local branch is not synchronized with origin/$currentBranch"
	Write-Host "   Local:  $localCommit" -ForegroundColor Gray
	Write-Host "   Remote: $remoteCommit" -ForegroundColor Gray

	$response = Read-Host "Do you want to continue anyway? (y/N)"
	if ($response -ne 'y' -and $response -ne 'Y') {
		Write-Host "❌ Operation canceled" -ForegroundColor Red
		exit 1
	}
}

# Verify that the tag does not exist
$existingTag = git tag -l $tag
if ($existingTag) {
	Write-Error "❌ The tag '$tag' already exists. Please choose another version."
	exit 1
}

# Show the latest commits
Write-Host "`n📝 Latest commits:" -ForegroundColor Cyan
git log --oneline -5

Write-Host "`n🏷️  The tag will be created: $tag" -ForegroundColor Green
Write-Host "📍 On commit: $(git rev-parse --short HEAD)" -ForegroundColor Gray

if ($DryRun) {
	Write-Host "`n🔍 DRY RUN - No changes will be made" -ForegroundColor Yellow
	Write-Host "Commands that would be executed:" -ForegroundColor Gray
	Write-Host "  git tag -a $tag -m 'Release $tag'" -ForegroundColor Gray
	Write-Host "  git push origin $tag" -ForegroundColor Gray
	exit 0
}

# Confirm
$confirmation = Read-Host "`nDo you want to create and publish version $tag? (y/N)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
	Write-Host "❌ Operation canceled" -ForegroundColor Red
	exit 1
}

# Create the tag
Write-Host "`n🏷️  Creating tag $tag..." -ForegroundColor Cyan
git tag -a $tag -m "Release $tag"

if ($LASTEXITCODE -ne 0) {
	Write-Error "❌ Error creating the tag"
	exit 1
}

# Publish the tag
Write-Host "📤 Publishing tag to the remote repository..." -ForegroundColor Cyan
git push origin $tag

if ($LASTEXITCODE -ne 0) {
	Write-Error "❌ Error publishing the tag. You can try manually with: git push origin $tag"
	exit 1
}

Write-Host "`n✅ Version $tag created and published successfully!" -ForegroundColor Green
Write-Host "🚀 The GitHub Actions workflow will start automatically" -ForegroundColor Cyan
Write-Host "📊 Monitor the progress at: https://github.com/evaristocuesta/Clovance/actions" -ForegroundColor Blue

Write-Host "`n📦 The images will be published at:" -ForegroundColor Cyan
Write-Host "   • ghcr.io/evaristocuesta/clovance/clovance-api:$Version" -ForegroundColor Gray
Write-Host "   • ghcr.io/evaristocuesta/clovance/clovance-frontend:$Version" -ForegroundColor Gray

if ($isPreRelease) {
	Write-Host "`n💡 This is a pre-release. Tags that will be created:" -ForegroundColor Yellow
	Write-Host "   • $Version (full version)" -ForegroundColor Gray
	Write-Host "   • $currentBranch-<sha> (commit SHA)" -ForegroundColor Gray
	Write-Host "`n⚠️  Note: Pre-releases DO NOT update production tags (major, minor, latest)" -ForegroundColor DarkYellow
} else {
	Write-Host "`n💡 Tip: The following tags will also be created:" -ForegroundColor Yellow
	$parts = $Version.Split('.')
	Write-Host "   • $Version (full version)" -ForegroundColor Gray
	Write-Host "   • $($parts[0]).$($parts[1]) (major.minor)" -ForegroundColor Gray
	Write-Host "   • $($parts[0]) (major)" -ForegroundColor Gray
	Write-Host "   • $currentBranch-<sha> (commit SHA)" -ForegroundColor Gray
	if ($currentBranch -eq "main") {
		Write-Host "   • latest (latest version)" -ForegroundColor Gray
	}
}
