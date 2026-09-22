// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Hyprism.Core.Models;

namespace Hyprism.Desktop.Screens.Instances;

public sealed partial class ModCatalogDependencyItemViewModel(
    ModDependency dependency,
    string unknownVersionLabel) : ObservableObject, IDisposable
{
    public string Id => dependency.ModId;
    public string Name => string.IsNullOrWhiteSpace(dependency.Name)
        ? dependency.ModId
        : dependency.Name;
    public string Version => string.IsNullOrWhiteSpace(dependency.Version)
        ? unknownVersionLabel
        : dependency.Version;
    public string VersionInParentheses => $"({Version})";
    public string IconUrl => dependency.IconUrl;
    public string Initial => string.IsNullOrWhiteSpace(Name)
        ? "M"
        : Name[..1].ToUpperInvariant();

    [ObservableProperty]
    private Bitmap? _icon;

    public bool ShowsIcon => Icon is not null;

    partial void OnIconChanged(Bitmap? value)
        => OnPropertyChanged(nameof(ShowsIcon));

    partial void OnIconChanging(Bitmap? value)
        => Icon?.Dispose();

    public void Dispose()
        => Icon = null;
}
