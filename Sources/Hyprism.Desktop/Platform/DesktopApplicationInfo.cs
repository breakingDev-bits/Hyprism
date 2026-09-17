// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Hyprism.Core.Infrastructure;

namespace Hyprism.Desktop.Platform;

internal static class DesktopApplicationInfo
{
    public static string Version { get; } =
        LauncherUserAgent.GetVersion(typeof(DesktopApplicationInfo).Assembly);

    public static string UserAgent { get; } = LauncherUserAgent.Create(Version);
}
