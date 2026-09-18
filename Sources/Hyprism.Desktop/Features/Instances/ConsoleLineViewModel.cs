// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

namespace Hyprism.Desktop.Features.Instances;

public sealed record ConsoleLineViewModel(string Level, string Time, string Text)
{
    public bool IsError => Level == "ERR";

    public bool IsWarning => Level == "WRN";

    public bool IsSystem => Level == "INF";
}

public sealed record InstanceListOptionViewModel(string Value, string Display);
