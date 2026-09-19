// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Hyprism.Core.Accounts;
using Hyprism.Core.Models;
using Hyprism.Desktop.Features.Profiles;
using Hyprism.Desktop.Localization;
using Hyprism.Desktop.Platform;
using Moq;
using Xunit;

namespace Hyprism.Desktop.Tests;

public sealed class ProfileWizardLifecycleTests
{
    [AvaloniaFact]
    public async Task CreatingProfileThenOpeningWizardAgainShowsAccountTypeChoice()
    {
        var profiles = new List<Profile>();
        var activeProfileId = string.Empty;
        var profileManager = new Mock<IProfileManager>();
        var profileRepository = new Mock<IProfileRepository>();
        var uriLauncher = new Mock<IExternalUriLauncher>();
        profileRepository.Setup(repository => repository.GetProfiles()).Returns(() => profiles.ToList());
        profileRepository.Setup(repository => repository.GetSelectedProfileId()).Returns(() => activeProfileId);
        profileRepository.Setup(repository => repository.CreateProfile(
                "New_Profile",
                It.IsAny<string>(),
                false))
            .Returns((string name, string uuid, bool _) =>
            {
                var profile = new Profile
                {
                    Id = "new-profile",
                    Name = name,
                    UUID = uuid
                };
                profiles.Add(profile);
                return profile;
            });
        profileRepository.Setup(repository => repository.SwitchProfile("new-profile"))
            .Callback(() => activeProfileId = "new-profile")
            .Returns(true);

        using var viewModel = new ProfilesViewModel(
            profileManager.Object,
            profileRepository.Object,
            uriLauncher.Object,
            new StringLocalizer("en-US"));
        var view = new ProfilesView { DataContext = viewModel };
        var window = new Window
        {
            Width = 1180,
            Height = 760,
            Content = view
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        viewModel.ShowCreateChoiceCommand.Execute(null);
        var choice = view.FindControl<StackPanel>("ProfileCreationChoiceContent");
        var offline = view.FindControl<StackPanel>("OfflineProfileCreationContent");
        Assert.NotNull(choice);
        Assert.NotNull(offline);
        await AvaloniaTestWait.UntilAsync(
            () => choice!.IsEffectivelyVisible && choice.Opacity >= 0.99,
            "profile creation choice to open");
        view.FindControl<Button>("BeginOfflineProfileCreationButton")!
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await AvaloniaTestWait.UntilAsync(
            () => viewModel.IsOfflineCreationVisible && offline!.IsEffectivelyVisible,
            "offline profile creation step to open");
        viewModel.OfflineProfileName = "New_Profile";
        viewModel.CreateOfflineProfileCommand.Execute(null);
        await AvaloniaTestWait.UntilAsync(
            () => !viewModel.IsCreationVisible &&
                  profiles.Count == 1 &&
                  !offline!.IsEffectivelyVisible,
            "offline profile creation to complete");

        viewModel.ShowCreateChoiceCommand.Execute(null);
        await AvaloniaTestWait.UntilAsync(
            () => viewModel.IsCreateChoiceVisible && choice!.IsEffectivelyVisible && choice.Opacity >= 0.99,
            "profile creation choice to reopen");

        Assert.NotNull(choice);
        Assert.True(choice!.IsEffectivelyVisible);
        Assert.Equal(1, choice.Opacity);
        Assert.Equal(0, Assert.IsType<TranslateTransform>(choice.RenderTransform).X);
        window.Close();
    }
}
