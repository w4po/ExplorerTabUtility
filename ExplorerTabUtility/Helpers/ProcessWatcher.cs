using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ExplorerTabUtility.Helpers;

public class ProcessEventArgs(int processId, string processName, int sessionId) : EventArgs
{
    public int ProcessId { get; } = processId;
    public int SessionId { get; } = sessionId;
    public string ProcessName { get; } = processName;
    public DateTime Timestamp { get; } = DateTime.Now;
}

public enum SessionFilter
{
    All,
    Current
}

/// <summary>
/// Watches for process creation and termination events
/// </summary>
public class ProcessWatcher : IDisposable
{
    private readonly string _processName;
    private readonly TimeSpan _scanInterval;
    private readonly SessionFilter _sessionFilter;
    private readonly int _currentSessionId;
    private readonly ConcurrentDictionary<int, (Process Process, int SessionId)> _trackedProcesses = new();
    private readonly Timer _scanTimer;
    private readonly object _scanLock = new();
    private readonly SynchronizationContext? _syncContext;
    private bool _isMonitoring = true;
    private bool _disposed;

    public event EventHandler<ProcessEventArgs>? ProcessCreated;
    public event EventHandler<ProcessEventArgs>? ProcessTerminated;

    /// <summary>
    /// Creates a new ProcessWatcher to monitor processes with the specified name
    /// </summary>
    /// <param name="processName">Name of the process to monitor (without .exe extension)</param>
    /// <param name="scanIntervalMs">How often to scan for processes in milliseconds</param>
    /// <param name="sessionFilter">Whether to monitor all sessions or only the current session</param>
    public ProcessWatcher(string processName, int scanIntervalMs = 500, SessionFilter sessionFilter = SessionFilter.Current)
    {
        _processName = processName ?? throw new ArgumentNullException(nameof(processName));
        _scanInterval = TimeSpan.FromMilliseconds(Math.Max(100, scanIntervalMs)); // Minimum 100ms
        _sessionFilter = sessionFilter;
        _currentSessionId = Process.GetCurrentProcess().SessionId;
        _syncContext = SynchronizationContext.Current;

        // Start scanning immediately, then at regular intervals
        _scanTimer = new Timer(ScanForProcesses, null, TimeSpan.Zero, _scanInterval);
    }

    /// <summary>
    /// Pauses the monitoring of processes
    /// </summary>
    public void Pause()
    {
        if (_disposed) return;
        _isMonitoring = false;
        _scanTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Resumes the monitoring of processes
    /// </summary>
    public void Resume()
    {
        if (_disposed) return;
        _isMonitoring = true;
        _scanTimer.Change(TimeSpan.Zero, _scanInterval);
    }

    private void ScanForProcesses(object? state)
    {
        if (_disposed || !_isMonitoring) return;

        // Use lock to prevent concurrent scans
        if (!Monitor.TryEnter(_scanLock)) return;

        try
        {
            // Check for terminated processes
            foreach (var pid in _trackedProcesses.Keys.ToList())
            {
                if (!_trackedProcesses.TryGetValue(pid, out var info)) continue;
        
                bool hasExited;
                try
                {
                    hasExited = info.Process.HasExited;
                }
                catch (Exception)
                {
                    hasExited = true;
                }
        
                if (hasExited && _trackedProcesses.TryRemove(pid, out _))
                {
                    // The Process no longer exists, but we didn't get the exit event
                    SafeCleanupAndNotifyTermination(pid, info);
                }
            }
            
            // Check all matching processes
            foreach (var process in Process.GetProcessesByName(_processName))
            {
                try
                {
                    var processId = process.Id;

                    // Skip if already tracking
                    if (_trackedProcesses.ContainsKey(processId))
                    {
                        SafeDisposeProcess(process);
                        continue;
                    }

                    // Skip if session doesn't match filter
                    if (_sessionFilter == SessionFilter.Current && process.SessionId != _currentSessionId)
                    {
                        SafeDisposeProcess(process);
                        continue;
                    }

                    // Found a new process to track
                    var sessionId = process.SessionId;
                    process.Exited += OnProcessExited;
                    process.EnableRaisingEvents = true;

                    if (!_trackedProcesses.TryAdd(processId, (process, sessionId))) continue;

                    // Successfully added - raise ProcessCreated event
                    var args = new ProcessEventArgs(processId, _processName, sessionId);
                    RaiseEvent(ProcessCreated, args);
                }
                catch (Exception)
                {
                    // Individual process handling failed
                    SafeDisposeProcess(process);
                }
            }
        }
        finally
        {
            Monitor.Exit(_scanLock);
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (_disposed || !_isMonitoring) return;
        if (sender is not Process process) return;

        try
        {
            var processId = process.Id;

            // Remove from tracking dictionary
            if (_trackedProcesses.TryRemove(processId, out var info))
            {
                // Cleanup and notify
                SafeCleanupAndNotifyTermination(processId, info);
            }
        }
        catch (Exception)
        {
            // Process object might be in an inconsistent state after exit
        }
    }

    private void SafeCleanupAndNotifyTermination(int processId, (Process Process, int SessionId) info)
    {
        try
        {
            // Unhook event
            info.Process.Exited -= OnProcessExited;

            // Create event args
            var args = new ProcessEventArgs(processId, _processName, info.SessionId);

            // Raise event
            RaiseEvent(ProcessTerminated, args);
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
        finally
        {
            // Always try to dispose
            SafeDisposeProcess(info.Process);
        }
    }

    private void RaiseEvent<T>(EventHandler<T>? eventHandler, T args) where T : EventArgs
    {
        if (eventHandler == null) return;

        // Raise event in the original synchronization context if available
        if (_syncContext != null)
        {
            _syncContext.Post(_ => eventHandler(this, args), null);
        }
        else
        {
            eventHandler(this, args);
        }
    }

    private void SafeDisposeProcess(Process process)
    {
        try
        {
            process.Dispose();
        }
        catch (Exception)
        {
            // Ignore disposal errors
        }
    }

    /// <summary>
    /// Releases all resources used by the ProcessWatcher
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _isMonitoring = false;

        // Stop timer
        try
        {
            _scanTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _scanTimer.Dispose();
        }
        catch
        {
            // ignored
        }

        // Clean up tracked processes
        foreach (var kvp in _trackedProcesses)
        {
            try
            {
                kvp.Value.Process.Exited -= OnProcessExited;
            }
            catch
            {
                // ignored
            }

            SafeDisposeProcess(kvp.Value.Process);
        }

        _trackedProcesses.Clear();
    }
}