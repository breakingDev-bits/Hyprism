// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Hyprism.Core.Models;

namespace Hyprism.Desktop.Screens.Instances;

public enum ModCatalogInstallState
{
    Pending,
    Installing,
    Installed,
    Failed
}

public sealed partial class ModCatalogInstallItemViewModel : ObservableObject
{
    private readonly Func<int, string> _dependencyCountFormatter;
    private readonly string _unknownVersionLabel;

    public ModCatalogInstallItemViewModel(
        ModCatalogItemViewModel catalogItem,
        Func<int, string> dependencyCountFormatter,
        string unknownVersionLabel)
    {
        CatalogItem = catalogItem;
        _dependencyCountFormatter = dependencyCountFormatter;
        _unknownVersionLabel = unknownVersionLabel;
        CatalogItem.PropertyChanged += OnCatalogItemPropertyChanged;
        SetDependencies(CatalogItem.Dependencies);
    }

    public ModCatalogItemViewModel CatalogItem { get; }
    public string Id => CatalogItem.Id;
    public string Name => CatalogItem.Name;
    public string Version => CatalogItem.RecommendedVersionLabel;
    public string Initial => CatalogItem.Initial;
    public bool ShowsIcon => CatalogItem.ShowsIcon;
    public Bitmap? Icon => CatalogItem.Icon;
    public ObservableCollection<ModCatalogDependencyItemViewModel> DependencyItems { get; } = [];
    public int DependencyCount => DependencyItems.Count;
    public bool HasDependencies => DependencyCount > 0;
    public string DependenciesLabel => _dependencyCountFormatter(DependencyCount);
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

    public void SetDependencies(IReadOnlyList<ModDependency> dependencies)
    {
        dependencies ??= [];
        foreach (var item in DependencyItems)
            item.Dispose();

        DependencyItems.Clear();
        foreach (var dependency in dependencies
                     .Where(dependency => dependency.RelationType == CurseForgeDependencyRelationType.RequiredDependency)
                     .DistinctBy(dependency => dependency.ModId, StringComparer.OrdinalIgnoreCase))
        {
            DependencyItems.Add(new ModCatalogDependencyItemViewModel(dependency, _unknownVersionLabel));
        }

        OnPropertyChanged(nameof(DependencyCount));
        OnPropertyChanged(nameof(HasDependencies));
        OnPropertyChanged(nameof(DependenciesLabel));
    }

    public void Dispose()
    {
        CatalogItem.PropertyChanged -= OnCatalogItemPropertyChanged;
        foreach (var item in DependencyItems)
            item.Dispose();
        DependencyItems.Clear();
    }

    private void OnCatalogItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(ModCatalogItemViewModel.Icon) or
            nameof(ModCatalogItemViewModel.ShowsIcon))
        {
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(ShowsIcon));
        }
        else if (args.PropertyName == nameof(ModCatalogItemViewModel.Dependencies))
        {
            SetDependencies(CatalogItem.Dependencies);
        }
    }
}
