// Copyright (C) 2026 HyPrism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HyPrism.Desktop.Controls;
using Xunit;

namespace HyPrism.Desktop.Tests;

public sealed class FadingComboBoxTests
{
    [AvaloniaFact]
    public void MouseWheelDoesNotChangeSelection()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "A category with a deliberately long name", "Gamma" }
        };
        comboBox.Classes.Add("uiComboBox");
        comboBox.SelectedIndex = 0;
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = comboBox
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var center = comboBox.TranslatePoint(
            new Point(comboBox.Bounds.Width / 2, comboBox.Bounds.Height / 2),
            window)!.Value;

        comboBox.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(comboBox.IsFocused);

        window.MouseWheel(center, new Vector(0, -1), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, comboBox.SelectedIndex);

        window.MouseWheel(center, new Vector(0, 1), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, comboBox.SelectedIndex);

        comboBox.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        Assert.Equal(comboBox.Bounds.Width, popup.Width);
        window.MouseWheel(center, new Vector(0, -1), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, comboBox.SelectedIndex);

        window.Close();
    }
}
