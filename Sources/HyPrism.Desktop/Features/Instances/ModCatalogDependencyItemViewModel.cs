// Copyright (C) 2026 HyPrism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using HyPrism.Core.Models;

namespace HyPrism.Desktop.Features.Instances;

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
