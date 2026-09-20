# Copyright (C) 2026 Hyprism Launcher
# SPDX-License-Identifier: GPL-3.0-only

$ErrorActionPreference = "Stop"

$GITHUB_RELEASE_BASE_URL = "https://github.com/hyprismteam/Hyprism/releases/latest/download"
$GITHUB_ICON_ICO_URL = "https://raw.githubusercontent.com/hyprismteam/Hyprism/main/Sources/Hyprism.Desktop/Assets/Images/Hyprism.ico"

$InstallDir = Join-Path $env:LOCALAPPDATA "HyPrism"
$DesktopDir = [Environment]::GetFolderPath("Desktop")

if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

Write-Host "--- OH-MY-HYPRISM INSTALLER ---"
Write-Host "[*] Fetching version info from $GITHUB_RELEASE_BASE_URL/version.json..."

try {
    $VersionJson = Invoke-RestMethod -Uri "$GITHUB_RELEASE_BASE_URL/version.json" -Headers @{"User-Agent"="HyPrism-Installer/1.0"}
    $Version = $VersionJson.version
    if (-not $Version) {
        throw "'version' field not found in remote version.json"
    }
} catch {
    Write-Error "[!] Failed to fetch version.json from GitHub releases: $_"
    exit 1
}

Write-Host "[*] Target Version: $Version"
Write-Host "[*] Detected Operating System: Windows ($env:PROCESSOR_ARCHITECTURE)"

$ArchiveName = "HyPrism-win-x64-$Version.zip"
$DownloadUrl = "$GITHUB_RELEASE_BASE_URL/$ArchiveName"
$ZipPath = Join-Path $InstallDir $ArchiveName
$TargetExePath = Join-Path $InstallDir "HyPrism.exe"
$IconPath = Join-Path $InstallDir "hyprism.ico"

Write-Host "[*] Downloading: $DownloadUrl -> $ZipPath"
Invoke-WebRequest -Uri $DownloadUrl -OutFile $ZipPath -Headers @{"User-Agent"="HyPrism-Installer/1.0"}

Write-Host "[*] Extracting $ZipPath..."
Expand-Archive -Path $ZipPath -DestinationPath $InstallDir -Force
Remove-Item -Path $ZipPath -Force

Write-Host "[*] Downloading launcher icon for Windows..."
Invoke-WebRequest -Uri $GITHUB_ICON_ICO_URL -OutFile $IconPath -Headers @{"User-Agent"="HyPrism-Installer/1.0"}

$ShortcutPath = Join-Path $DesktopDir "HyPrism Launcher.lnk"
Write-Host "[*] Creating Desktop shortcut: $ShortcutPath"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = $TargetExePath
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.IconLocation = $IconPath
$Shortcut.Save()
Write-Host "[+] Windows Desktop shortcut created successfully!"

$UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
$PathComponents = $UserPath -split ";"
if ($InstallDir -notin $PathComponents) {
    $NewPath = if ($UserPath) { "$UserPath;$InstallDir" } else { $InstallDir }
    [Environment]::SetEnvironmentVariable("Path", $NewPath, "User")
    Write-Host "[+] Added $InstallDir to user PATH (please restart your terminal)."
}

Write-Host "[+] Windows installation complete! Executable saved to: $TargetExePath"