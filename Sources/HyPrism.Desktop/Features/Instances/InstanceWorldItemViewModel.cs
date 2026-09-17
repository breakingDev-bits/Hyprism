// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

namespace Hyprism.Desktop.Features.Instances;

public sealed record InstanceWorldItemViewModel(
    string Name,
    string LastModified,
    string Size)
{
    public string Initial => string.IsNullOrWhiteSpace(Name)
        ? "W"
        : Name[..1].ToUpperInvariant();
}
