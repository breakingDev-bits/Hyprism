// Copyright (C) 2026 HyPrism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using HyPrism.Desktop.Shell;
using Xunit;

namespace HyPrism.Desktop.Tests;

public sealed class MainWindowResizeTests
{
    [AvaloniaFact]
    public void CustomChromeUsesAvaloniaDecorationHitTesting()
    {
        var window = new MainWindow();

        try
        {
            Assert.True(window.CanResize);
            Assert.True(window.ExtendClientAreaToDecorationsHint);
            var expectedDecorations = OperatingSystem.IsWindows()
                ? WindowDecorations.Full
                : WindowDecorations.None;
            Assert.Equal(expectedDecorations, window.WindowDecorations);
            Assert.IsType<ControlTheme>(window.FindResource(typeof(WindowDrawnDecorations)));

            AssertElementRole(window, "ResizeNorth", WindowDecorationsElementRole.ResizeN);
            AssertElementRole(window, "ResizeSouth", WindowDecorationsElementRole.ResizeS);
            AssertElementRole(window, "ResizeWest", WindowDecorationsElementRole.ResizeW);
            AssertElementRole(window, "ResizeEast", WindowDecorationsElementRole.ResizeE);
            AssertElementRole(window, "ResizeNorthWest", WindowDecorationsElementRole.ResizeNW);
            AssertElementRole(window, "ResizeNorthEast", WindowDecorationsElementRole.ResizeNE);
            AssertElementRole(window, "ResizeSouthWest", WindowDecorationsElementRole.ResizeSW);
            AssertElementRole(window, "ResizeSouthEast", WindowDecorationsElementRole.ResizeSE);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertElementRole(
        MainWindow window,
        string name,
        WindowDecorationsElementRole expectedRole)
    {
        var element = Assert.IsAssignableFrom<Avalonia.Visual>(window.FindControl<Control>(name));
        Assert.Equal(expectedRole, WindowDecorationProperties.GetElementRole(element));
    }
}
