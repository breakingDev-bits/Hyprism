// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Hyprism.Desktop.Tests.TestAppBuilder))]

namespace Hyprism.Desktop.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false
            });
}
