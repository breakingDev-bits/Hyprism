// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Hyprism.Desktop.Controls;
using Xunit;

namespace Hyprism.Desktop.Tests;

public sealed class EmptyStateTests
{
    [AvaloniaFact]
    public void TemplatePresentsFeatureContentAndAction()
    {
        var state = new EmptyState
        {
            Title = "No instances",
            Description = "Choose a version to create one",
            Icon = new TextBlock { Text = "icon" },
            ActionContent = new Button { Content = "Create" }
        };
        var window = new Window
        {
            Width = 420,
            Height = 260,
            Content = state
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var textBlocks = state.GetVisualDescendants().OfType<TextBlock>().ToArray();
        Assert.Contains(textBlocks, text => text.Text == "No instances");
        Assert.Contains(textBlocks, text => text.Text == "Choose a version to create one");
        Assert.Contains(textBlocks, text => text.Text == "icon");
        Assert.Contains(state.GetVisualDescendants().OfType<Button>(), button => button.Content is "Create");

        window.Close();
    }
}
