#!/bin/sh

# Copyright (C) 2026 Hyprism Launcher
# SPDX-License-Identifier: GPL-3.0-only

set -e

case "$(uname -s 2>/dev/null)" in
    CYGWIN*|MINGW*|MSYS*)
        echo "[*] Detected Windows environment. Switching to PowerShell script..."
        exec powershell.exe -NoProfile -ExecutionPolicy Bypass -File "./install.ps1" "$@"
        ;;
esac

GITHUB_RELEASE_BASE_URL="https://github.com/hyprismteam/Hyprism/releases/latest/download"
GITHUB_ICON_PNG_URL="https://raw.githubusercontent.com/hyprismteam/Hyprism/main/Sources/Hyprism.Desktop/Assets/Images/logo.svg"

BIN_DIR="$HOME/.local/bin"
APPLICATIONS_DIR="$HOME/.local/share/applications"
ICONS_DIR="$HOME/.local/share/icons/hicolor/256x256/apps"

mkdir -p "$BIN_DIR" "$APPLICATIONS_DIR" "$ICONS_DIR"

echo "--- OH-MY-HYPRISM INSTALLER ---"
echo "[*] Fetching version info from ${GITHUB_RELEASE_BASE_URL}/version.json..."

VERSION_JSON=$(curl -fsSL "${GITHUB_RELEASE_BASE_URL}/version.json" 2>/dev/null || wget -qO- "${GITHUB_RELEASE_BASE_URL}/version.json")
if [ -z "$VERSION_JSON" ]; then
    echo "[!] Failed to fetch version.json from GitHub releases." >&2
    exit 1
fi

if command -v jq >/dev/null 2>&1; then
    VERSION=$(echo "$VERSION_JSON" | jq -r '.version')
else
    VERSION=$(echo "$VERSION_JSON" | grep -o '"version"[^,]*' | sed -E 's/.*"([^"]+)": *"([^"]+)".*/\2/')
fi

if [ -z "$VERSION" ] || [ "$VERSION" = "null" ]; then
    echo "[!] Error: 'version' field not found in remote version.json" >&2
    exit 1
fi

echo "[*] Target Version: $VERSION"
echo "[*] Detected Operating System: $(uname -s) ($(uname -m))"

ARCHIVE_NAME="HyPrism-linux-x64-${VERSION}.tar.xz"
DOWNLOAD_URL="${GITHUB_RELEASE_BASE_URL}/${ARCHIVE_NAME}"
TAR_PATH="${BIN_DIR}/${ARCHIVE_NAME}"
TARGET_BIN_PATH="${BIN_DIR}/hyprism"
ICON_PATH="${ICONS_DIR}/hyprism.png"

echo "[*] Downloading: ${DOWNLOAD_URL} -> ${TAR_PATH}"
if command -v curl >/dev/null 2>&1; then
    curl -fL -o "$TAR_PATH" "$DOWNLOAD_URL"
else
    wget -O "$TAR_PATH" "$DOWNLOAD_URL"
fi

echo "[*] Extracting archive ${TAR_PATH}..."
tar -xf "$TAR_PATH" -C "$BIN_DIR"
rm -f "$TAR_PATH"

if [ -f "$TARGET_BIN_PATH" ]; then
    chmod +x "$TARGET_BIN_PATH"
fi

echo "[*] Downloading launcher icon..."
if command -v curl >/dev/null 2>&1; then
    curl -fL -o "$ICON_PATH" "$GITHUB_ICON_PNG_URL"
else
    wget -O "$ICON_PATH" "$GITHUB_ICON_PNG_URL"
fi

if [ "$(uname -s)" = "Linux" ]; then
    DESKTOP_FILE="${APPLICATIONS_DIR}/hyprism.desktop"
    cat <<EOF > "$DESKTOP_FILE"
[Desktop Entry]
Type=Application
Name=HyPrism Launcher
Comment=HyPrism Game Launcher
Exec=${TARGET_BIN_PATH}
Icon=${ICON_PATH}
Terminal=false
Categories=Game;Utility;
EOF
    chmod +x "$DESKTOP_FILE"
    echo "[+] Created .desktop shortcut: ${DESKTOP_FILE}"
fi

echo "[+] POSIX installation complete! Executable located at: ${TARGET_BIN_PATH}"