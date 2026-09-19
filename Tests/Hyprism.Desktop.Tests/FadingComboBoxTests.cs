// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Hyprism.Desktop.Controls;
using Xunit;

namespace Hyprism.Desktop.Tests;

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

    [AvaloniaFact]
    public async Task ComboBoxUsesReferenceTrackColorsWhenHovered()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = comboBox
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var comboBackground = comboBox.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Name == "Background");
        Assert.Equal(
            Color.Parse("#292A2D"),
            Assert.IsAssignableFrom<ISolidColorBrush>(comboBackground.Background).Color);

        var center = comboBox.TranslatePoint(
            new Point(comboBox.Bounds.Width / 2, comboBox.Bounds.Height / 2),
            window)!.Value;
        window.MouseMove(center);
        await WaitUntilAsync(
            () => comboBackground.Background is ISolidColorBrush brush &&
                  brush.Color == Color.Parse("#3B3E43"));

        window.Close();
    }

    [AvaloniaFact]
    public async Task ClickingAnOpenComboBoxClosesItInsteadOfReopeningIt()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
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
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());

        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.True(comboBox.IsDropDownOpen);
        Assert.True(popup.IsRequestedOpen);

        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.False(comboBox.IsDropDownOpen);
        Assert.False(popup.IsRequestedOpen);

        await WaitUntilAsync(() => !popup.IsOpen);
        window.Close();
    }

    [AvaloniaFact]
    public async Task PopupPlacementKeepsEightPixelGapWhenPlacedAbove()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        Canvas.SetLeft(comboBox, 40);
        Canvas.SetTop(comboBox, 245);

        var canvas = new Canvas
        {
            Width = 420,
            Height = 320
        };
        canvas.Children.Add(comboBox);

        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = canvas
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(
            () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                  Math.Abs(gap - 8) < 0.1);

        Assert.Equal(-8, popup.VerticalOffset);
        Assert.Equal(PlacementMode.Top, popup.Placement);
        Assert.True(TryGetVerticalGap(comboBox, popup, window, out var verticalGap));
        Assert.Equal(8, verticalGap, precision: 3);
        window.Close();
    }

    [AvaloniaFact]
    public async Task PopupPlacementKeepsEightPixelGapWhenPlacedBelow()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        Canvas.SetLeft(comboBox, 40);
        Canvas.SetTop(comboBox, 0);

        var canvas = new Canvas
        {
            Width = 420,
            Height = 320
        };
        canvas.Children.Add(comboBox);

        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = canvas
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        Assert.True(
            TryGetVerticalGap(comboBox, popup, window, out var observedGap),
            $"offset={popup.VerticalOffset} gap={observedGap}");
        Assert.Equal(8, observedGap, precision: 3);

        Assert.Equal(8, popup.VerticalOffset);
        Assert.True(TryGetVerticalGap(comboBox, popup, window, out var verticalGap));
        Assert.Equal(8, verticalGap, precision: 3);
        window.Close();
    }

    [AvaloniaFact]
    public async Task PopupPlacementKeepsEightPixelGapWhenPlacedBelowInsideScrollViewer()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");

        var content = new StackPanel
        {
            Children =
            {
                comboBox,
                new Border { Height = 500 }
            }
        };
        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 300,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Content = content
        };
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(
            () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                  Math.Abs(gap - 8) < 0.1);

        Assert.Equal(8, popup.VerticalOffset);
        Assert.False(ScrollViewer.GetBringIntoViewOnFocusChange(scrollViewer));
        comboBox.IsDropDownOpen = false;
        await WaitUntilAsync(() => !popup.IsOpen);
        Assert.False(ScrollViewer.GetBringIntoViewOnFocusChange(scrollViewer));
        window.Close();
    }

    [AvaloniaFact]
    public async Task ScrollingDoesNotFlipAnOpenPopupToTheOtherSide()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        Canvas.SetLeft(comboBox, 40);
        Canvas.SetTop(comboBox, 245);

        var canvas = new Canvas
        {
            Width = 420,
            Height = 600
        };
        canvas.Children.Add(comboBox);

        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 300,
            Content = canvas
        };
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(
            () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                  Math.Abs(gap - 8) < 0.1);
        Assert.Equal(PlacementMode.Top, popup.Placement);

        scrollViewer.Offset = new Vector(0, 180);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(PlacementMode.Top, popup.Placement);
        Assert.Equal(-8, popup.VerticalOffset);
        Assert.True(TryGetVerticalGap(comboBox, popup, window, out var scrolledGap));
        Assert.Equal(8, scrolledGap, precision: 3);
        window.Close();
    }

    [AvaloniaFact]
    public void PointerFocusDoesNotScrollItsParent()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");

        var content = new StackPanel
        {
            Spacing = 0,
            Children =
            {
                new Border { Height = 190 },
                comboBox,
                new Border { Height = 180 }
            }
        };
        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 220,
            BringIntoViewOnFocusChange = true,
            Content = content
        };
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var before = scrollViewer.Offset;
        comboBox.Focus(NavigationMethod.Pointer, KeyModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(comboBox.IsFocused);
        Assert.Equal(before, scrollViewer.Offset);
        window.Close();
    }

    [AvaloniaFact]
    public async Task PopupStaysClippedToItsContentContextWhileThePageScrolls()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        Canvas.SetLeft(comboBox, 40);
        Canvas.SetTop(comboBox, 170);

        var canvas = new Canvas
        {
            Width = 420,
            Height = 600
        };
        canvas.Children.Add(comboBox);

        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 220,
            Content = canvas
        };
        var window = new Window
        {
            Width = 420,
            Height = 400,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(
            () => popup.Child?.Clip is RectangleGeometry geometry &&
                  geometry.Rect.Width > 0 &&
                  geometry.Rect.Height > 0);

        Assert.Equal(PlacementMode.Top, popup.Placement);
        var popupChild = Assert.IsAssignableFrom<Visual>(popup.Child);
        var initialClip = Assert.IsType<RectangleGeometry>(popupChild.Clip).Rect;

        scrollViewer.Offset = new Vector(0, 100);
        Dispatcher.UIThread.RunJobs();

        var scrolledClip = Assert.IsType<RectangleGeometry>(popupChild.Clip).Rect;
        Assert.NotEqual(initialClip, scrolledClip);
        Assert.True(scrolledClip.Top > initialClip.Top);
        Assert.True(scrolledClip.Right <= popupChild.Bounds.Width);
        Assert.True(scrolledClip.Bottom <= popupChild.Bounds.Height);
        window.Close();
    }

    [AvaloniaFact]
    public async Task PopupUsesThePageViewportInsteadOfTheImmediateClippedContainer()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");

        var clippedRow = new Border
        {
            Width = 220,
            Height = 44,
            ClipToBounds = true,
            Child = comboBox
        };
        var content = new StackPanel
        {
            Children =
            {
                clippedRow,
                new Border { Height = 500 }
            }
        };
        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 220,
            Content = content
        };
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(
            () => popup.Child?.Clip is RectangleGeometry geometry &&
                  geometry.Rect.Width > 0 &&
                  geometry.Rect.Height > 0);

        Assert.Equal(PlacementMode.Bottom, popup.Placement);
        window.Close();
    }

    [AvaloniaFact]
    public void PointerClickDoesNotScrollItsParent()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");

        var content = new StackPanel
        {
            Children =
            {
                new Border { Height = 190 },
                comboBox,
                new Border { Height = 180 }
            }
        };
        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 220,
            BringIntoViewOnFocusChange = true,
            Content = content
        };
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var before = scrollViewer.Offset;
        var center = comboBox.TranslatePoint(
            new Point(comboBox.Bounds.Width / 2, comboBox.Bounds.Height / 2),
            window)!.Value;
        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(comboBox.IsDropDownOpen);
        Assert.Equal(
            before,
            scrollViewer.Offset);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ClickingPopupItemDoesNotScrollItsParent()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");

        var content = new StackPanel
        {
            Children =
            {
                new Border { Height = 190 },
                comboBox,
                new Border { Height = 180 }
            }
        };
        var scrollViewer = new SmoothScrollViewer
        {
            Width = 420,
            Height = 220,
            Content = content
        };
        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = scrollViewer
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(() => popup.IsOpen);
        var popupChild = Assert.IsAssignableFrom<Visual>(popup.Child);
        var item = Assert.Single(
            popupChild.GetVisualDescendants().OfType<ComboBoxItem>(),
            container => Equals(container.Content, "Beta"));
        var itemCenter = item.TranslatePoint(
            new Point(item.Bounds.Width / 2, item.Bounds.Height / 2),
            window)!.Value;
        var before = scrollViewer.Offset;

        window.MouseDown(itemCenter, MouseButton.Left);
        window.MouseUp(itemCenter, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(before, scrollViewer.Offset);
        Assert.False(comboBox.IsDropDownOpen);
        await WaitUntilAsync(() => !popup.IsOpen);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ReopeningPopupRecalculatesItsSideAndKeepsTheGap()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        Canvas.SetLeft(comboBox, 40);
        Canvas.SetTop(comboBox, 245);

        var canvas = new Canvas
        {
            Width = 420,
            Height = 320
        };
        canvas.Children.Add(comboBox);

        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = canvas
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        for (var cycle = 0; cycle < 3; cycle++)
        {
            comboBox.IsDropDownOpen = true;
            await WaitUntilAsync(
                () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                      Math.Abs(gap - 8) < 0.1);

            Assert.Equal(-8, popup.VerticalOffset);
            Assert.Equal(PlacementMode.Top, popup.Placement);
            comboBox.IsDropDownOpen = false;
            await WaitUntilAsync(() => !popup.IsOpen);
        }

        Canvas.SetTop(comboBox, 0);
        Dispatcher.UIThread.RunJobs();
        comboBox.IsDropDownOpen = true;
        await WaitUntilAsync(
            () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                  Math.Abs(gap - 8) < 0.1);

        Assert.Equal(8, popup.VerticalOffset);
        Assert.Equal(PlacementMode.Bottom, popup.Placement);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ReopeningWhileCloseAnimationIsPendingKeepsTheGap()
    {
        var comboBox = new FadingComboBox
        {
            Width = 220,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" },
            SelectedIndex = 0
        };
        comboBox.Classes.Add("uiComboBox");
        Canvas.SetLeft(comboBox, 40);
        Canvas.SetTop(comboBox, 245);

        var canvas = new Canvas
        {
            Width = 420,
            Height = 320
        };
        canvas.Children.Add(comboBox);

        var window = new Window
        {
            Width = 420,
            Height = 320,
            Content = canvas
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        comboBox.IsDropDownOpen = true;
        var popup = Assert.Single(comboBox.GetVisualDescendants().OfType<FadingPopup>());
        await WaitUntilAsync(
            () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                  Math.Abs(gap - 8) < 0.1);
        Assert.Equal(-8, popup.VerticalOffset);
        Assert.Equal(PlacementMode.Top, popup.Placement);

        comboBox.IsDropDownOpen = false;
        comboBox.IsDropDownOpen = true;
        await WaitUntilAsync(
            () => TryGetVerticalGap(comboBox, popup, window, out var gap) &&
                  Math.Abs(gap - 8) < 0.1);

        Assert.Equal(-8, popup.VerticalOffset);
        Assert.Equal(PlacementMode.Top, popup.Placement);
        window.Close();
    }

    private static bool TryGetVerticalGap(
        FadingComboBox comboBox,
        FadingPopup popup,
        Window window,
        out double gap)
    {
        gap = 0;
        if (popup.Child is not Visual popupChild)
            return false;

        var targetOrigin = comboBox.TranslatePoint(new Point(), window);
        var popupOrigin = popupChild.TranslatePoint(new Point(), window);
        if (targetOrigin is not { } targetPoint || popupOrigin is not { } popupPoint)
            return false;

        var targetBottom = targetPoint.Y + comboBox.Bounds.Height;
        var popupBottom = popupPoint.Y + popupChild.Bounds.Height;
        gap = popupPoint.Y < targetPoint.Y
            ? targetPoint.Y - popupBottom
            : popupPoint.Y - targetBottom;
        return true;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
                return;

            await Task.Delay(10);
        }

        Assert.True(condition());
    }
}
