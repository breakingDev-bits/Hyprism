// Copyright (C) 2026 Hyprism Launcher
// SPDX-License-Identifier: GPL-3.0-only

using Hyprism.Core.Models;
using Hyprism.Core.Application.Ports;

namespace Hyprism.Core.Application.Progress;

/// <summary>
/// Manages progress notifications for downloads and installations.
/// Coordinates with Discord Rich Presence to reflect current activity
/// </summary>
public sealed class ProgressReporter : IProgressReporter
{
    private const int BroadcastIntervalMilliseconds = 100;

    private readonly IDiscordPresence _discord;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _broadcastGate = new();
    private readonly AsyncLocal<string?> _operationInstanceId = new();
    private readonly Dictionary<string, BroadcastState> _broadcastStates =
        new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public event Action<ProgressUpdateMessage>? DownloadProgressChanged;

    /// <inheritdoc/>
    public event Action<OperationErrorMessage>? OperationErrorOccurred;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressReporter"/> class
    /// </summary>
    /// <param name="discord">The Discord service for Rich Presence updates</param>
    /// <param name="timeProvider">The clock used for progress throttling</param>
    public ProgressReporter(IDiscordPresence discord, TimeProvider? timeProvider = null)
    {
        _discord = discord;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc/>
    public IDisposable BeginOperation(string instanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        var previousInstanceId = _operationInstanceId.Value;
        _operationInstanceId.Value = instanceId;
        return new OperationScope(_operationInstanceId, previousInstanceId);
    }

    private void SendProgress(string stage, int progress, string messageKey, object[]? args, long downloaded, long total, string? instanceId)
    {
        var msg = new ProgressUpdateMessage
        {
            InstanceId = instanceId ?? _operationInstanceId.Value,
            State = stage,
            Progress = progress,
            MessageKey = messageKey,
            Args = args,
            DownloadedBytes = downloaded,
            TotalBytes = total
        };

        DownloadProgressChanged?.Invoke(msg);

        // Don't update Discord during download/install to avoid showing extraction messages
        // Only update on complete or idle
        if (stage == "complete")
        {
            _discord.SetPresence(PresenceState.Idle);
        }
    }

    /// <inheritdoc/>
    public void ReportDownloadProgress(string stage, int progress, string messageKey, object[]? args = null, long downloaded = 0, long total = 0, string? instanceId = null)
    {
        // Download loops report on every buffer read; broadcast at most about 10 updates
        // per second per stage, always letting stage changes and completion through
        if (!ShouldBroadcast(stage, progress, instanceId))
            return;

        SendProgress(stage, progress, messageKey, args, downloaded, total, instanceId);
    }

    private bool ShouldBroadcast(string stage, int progress, string? instanceId)
    {
        lock (_broadcastGate)
        {
            var now = _timeProvider.GetTimestamp();
            var operationId = instanceId ?? _operationInstanceId.Value ?? "unscoped";
            _broadcastStates.TryGetValue(operationId, out var previous);
            var stageChanged = !string.Equals(stage, previous?.Stage, StringComparison.Ordinal);
            var isTerminal = progress >= 100;
            var intervalElapsed = previous is null ||
                                  _timeProvider.GetElapsedTime(previous.LastBroadcastTimestamp) >=
                                  TimeSpan.FromMilliseconds(BroadcastIntervalMilliseconds);

            if (!stageChanged && !isTerminal && !intervalElapsed)
                return false;

            _broadcastStates[operationId] = new BroadcastState(stage, now);
            return true;
        }
    }

    /// <summary>
    /// Sends an error notification to subscribed listeners
    /// </summary>
    /// <param name="type">The error category</param>
    /// <param name="message">The user-facing error message</param>
    /// <param name="technical">Optional diagnostic details</param>
    /// <param name="instanceId">Optional instance identifier associated with the error</param>
    private void SendErrorEvent(string type, string message, string? technical, string? instanceId)
    {
        OperationErrorOccurred?.Invoke(new OperationErrorMessage
        {
            InstanceId = instanceId ?? _operationInstanceId.Value,
            Type = type,
            Message = message,
            Technical = technical
        });
    }

    /// <inheritdoc/>
    public void ReportError(string type, string message, string? technical = null, string? instanceId = null)
        => SendErrorEvent(type, message, technical, instanceId);

    private sealed class OperationScope(AsyncLocal<string?> instanceId, string? previousInstanceId) : IDisposable
    {
        private readonly AsyncLocal<string?> _instanceId = instanceId;
        private readonly string? _previousInstanceId = previousInstanceId;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _instanceId.Value = _previousInstanceId;
            _disposed = true;
        }
    }

    private sealed record BroadcastState(string Stage, long LastBroadcastTimestamp);
}
