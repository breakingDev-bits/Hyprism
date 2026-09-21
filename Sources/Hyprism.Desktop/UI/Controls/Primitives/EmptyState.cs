//
// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only
//

using Avalonia;
using Avalonia.Controls;

namespace Hyprism.Desktop.Controls;

/// <summary>
/// Presents a consistent empty-state hierarchy while allowing the feature to provide its own icon and action
/// </summary>
public sealed class EmptyState : ContentControl
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<EmptyState, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Description));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<EmptyState, object?>(nameof(ActionContent));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }
}
