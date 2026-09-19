// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Hyprism.Core.Accounts;
using Hyprism.Core.Models;
using Hyprism.Desktop.Controls;
using Hyprism.Desktop.Features.Profiles;
using Hyprism.Desktop.Localization;
using Hyprism.Desktop.Platform;
using Moq;
using Xunit;

namespace Hyprism.Desktop.Tests;

public sealed class WizardAnimationTests
{
    [AvaloniaFact]
    public async Task NavigationPaneTransitionAnimatesWidthAndOpacity()
    {
        var overview = new Border { RenderTransform = new TranslateTransform() };
        var wizard = new Border { RenderTransform = new TranslateTransform() };
        var pane = new Border
        {
            Width = 276,
            RenderTransform = new TranslateTransform(),
            Transitions = new Transitions
            {
                new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromMilliseconds(190) },
                new DoubleTransition { Property = Layoutable.WidthProperty, Duration = TimeSpan.FromMilliseconds(190) }
            }
        };
        Assert.IsType<TranslateTransform>(pane.RenderTransform).Transitions = new Transitions
        {
            new DoubleTransition { Property = TranslateTransform.XProperty, Duration = TimeSpan.FromMilliseconds(190) }
        };
        var window = new Window
        {
            Width = 1000,
            Height = 700,
            Content = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                Children = { pane, wizard }
            }
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var transition = new WizardScreenTransition(overview, wizard, pane);
        transition.HideNavigationPane(animate: true);
        await WaitForRenderStateAsync(
            () => pane.Opacity is > 0.01 and < 0.99 &&
                  pane.Bounds.Width is > 0.5 and < 275.5,
            "navigation pane transition to reach an intermediate state");

        window.Close();
    }

    [AvaloniaFact]
    public async Task CompactProfileCreatorOpeningKeepsItsDetailSlide()
    {
        var profileManager = new Mock<IProfileManager>();
        var profileRepository = new Mock<IProfileRepository>();
        profileRepository.Setup(repository => repository.GetProfiles()).Returns(
        [
            new Profile { Id = "active", Name = "Active", UUID = Guid.NewGuid().ToString() }
        ]);
        profileRepository.Setup(repository => repository.GetSelectedProfileId()).Returns("active");

        using var profilesViewModel = new ProfilesViewModel(
            profileManager.Object,
            profileRepository.Object,
            new Mock<IExternalUriLauncher>().Object,
            new StringLocalizer("en-US"));
        var view = new ProfilesView { DataContext = profilesViewModel };
        var window = new Window
        {
            Width = 760,
            Height = 760,
            Content = view
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var profileMain = Assert.IsType<Grid>(view.FindControl<Grid>("ProfileMain"));
        var translation = Assert.IsType<TranslateTransform>(profileMain.RenderTransform);
        var initialOffset = profileMain.Bounds.Width;
        Assert.InRange(translation.X, initialOffset - 1, initialOffset + 1);

        var observedIntermediateOffset = false;
        void OnTranslationChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property == TranslateTransform.XProperty &&
                translation.X > 0.5 &&
                translation.X < initialOffset - 0.5)
            {
                observedIntermediateOffset = true;
            }
        }

        translation.PropertyChanged += OnTranslationChanged;
        try
        {
            var addProfileRow = view.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => button.IsEffectivelyVisible && button.Classes.Contains("instancesAddRow"));
            addProfileRow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await WaitForRenderStateAsync(
                () => observedIntermediateOffset,
                "compact profile creator opening slide to advance");
            await WaitForRenderStateAsync(
                () => Math.Abs(translation.X) <= 0.01,
                "compact profile creator opening slide to finish");
        }
        finally
        {
            translation.PropertyChanged -= OnTranslationChanged;
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task ProfileRefreshDoesNotSnapNavigationPaneDuringWizardClose()
    {
        var profileManager = new Mock<IProfileManager>();
        var profileRepository = new Mock<IProfileRepository>();
        profileRepository.Setup(repository => repository.GetProfiles()).Returns(
        [
            new Profile { Id = "active", Name = "Active", UUID = Guid.NewGuid().ToString() }
        ]);
        profileRepository.Setup(repository => repository.GetSelectedProfileId()).Returns("active");

        using var profilesViewModel = new ProfilesViewModel(
            profileManager.Object,
            profileRepository.Object,
            new Mock<IExternalUriLauncher>().Object,
            new StringLocalizer("en-US"));
        var view = new ProfilesView { DataContext = profilesViewModel };
        var window = new Window
        {
            Width = 1180,
            Height = 760,
            Content = view
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        var pane = Assert.IsType<Border>(view.FindControl<Border>("ProfilesListPane"));
        Assert.Equal(276, pane.Bounds.Width);

        profilesViewModel.ShowCreateChoiceCommand.Execute(null);
        await WaitForRenderStateAsync(
            () => pane.Bounds.Width <= 0.5,
            "navigation pane to finish hiding before wizard close");

        profilesViewModel.CancelCreationCommand.Execute(null);
        profileRepository.Raise(repository => repository.ProfilesChanged += null);
        await WaitForRenderStateAsync(
            () => pane.Bounds.Width is > 0.5 and < 275.5,
            "navigation pane to start reopening without snapping");
        Assert.InRange(pane.Bounds.Width, 0.5, 275.5);

        await WaitForRenderStateAsync(
            () => pane.Bounds.Width >= 275.5 && pane.Opacity >= 0.99,
            "navigation pane to finish reopening");

        window.Close();
    }

    private static async Task WaitForRenderStateAsync(Func<bool> condition, string description)
    {
        var startedAt = Stopwatch.GetTimestamp();
        while (!condition() && Stopwatch.GetElapsedTime(startedAt) < TimeSpan.FromSeconds(5))
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(16, TestContext.Current.CancellationToken);
        }

        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
        Assert.True(condition(), $"Timed out waiting for {description}");
    }
}
