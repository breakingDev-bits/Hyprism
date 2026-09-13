// Copyright (C) 2026 HyPrism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HyPrism.Desktop.Features.Instances;

public enum ModCatalogInstallState
{
    Pending,
    Installing,
    Installed,
    Failed
}

public sealed partial class ModCatalogInstallItemViewModel : ObservableObject
{
    public ModCatalogInstallItemViewModel(ModCatalogItemViewModel catalogItem)
    {
        CatalogItem = catalogItem;
        CatalogItem.PropertyChanged += OnCatalogItemPropertyChanged;
    }

    public ModCatalogItemViewModel CatalogItem { get; }
    public string Id => CatalogItem.Id;
    public string Name => CatalogItem.Name;
    public string Version => CatalogItem.RecommendedVersionLabel;
    public string Initial => CatalogItem.Initial;
    public bool ShowsIcon => CatalogItem.ShowsIcon;
    public Bitmap? Icon => CatalogItem.Icon;
    public bool IsProgressVisible => State is not ModCatalogInstallState.Pending;
    public string ProgressText => State switch
    {
        ModCatalogInstallState.Installed => "100%",
        _ => $"{Progress:0}%"
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressVisible))]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private ModCatalogInstallState _state = ModCatalogInstallState.Pending;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private double _progress;

    public void Begin()
    {
        State = ModCatalogInstallState.Installing;
        Progress = 5;
    }

    public void SetProgress(double progress)
    {
        Progress = Math.Clamp(progress, 0, 100);
        State = ModCatalogInstallState.Installing;
    }

    public void Complete()
    {
        Progress = 100;
        State = ModCatalogInstallState.Installed;
    }

    public void Fail()
        => State = ModCatalogInstallState.Failed;

    public void Dispose()
        => CatalogItem.PropertyChanged -= OnCatalogItemPropertyChanged;

    private void OnCatalogItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(ModCatalogItemViewModel.Icon) or
            nameof(ModCatalogItemViewModel.ShowsIcon))
        {
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(ShowsIcon));
        }
    }
}
