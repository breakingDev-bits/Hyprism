// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using Hyprism.Core.Models;

namespace Hyprism.Core.Tests.Models;

public sealed class InstanceInfoTests
{
    [Fact]
    public void InstallationStateIsRuntimeOnlyAndDefaultsToFalse()
    {
        var instance = new InstanceInfo
        {
            Id = "instance-id",
            Name = "Instance",
            IsInstalled = true
        };

        var json = JsonSerializer.Serialize(instance);
        var restored = JsonSerializer.Deserialize<InstanceInfo>(
            """{"Id":"instance-id","Name":"Instance","IsInstalled":true}""");

        Assert.DoesNotContain("IsInstalled", json, StringComparison.Ordinal);
        Assert.NotNull(restored);
        Assert.False(restored!.IsInstalled);
    }
}
