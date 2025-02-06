using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExplorerTabUtility.WinAPI;

namespace ExplorerTabUtility.Helpers;

public static class Helper
{
    public static Task DoDelayedBackgroundAsync(Action action, int delayMs = 2_000, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            action();
        }, cancellationToken);
    }
    public static Task DoDelayedBackgroundAsync(Func<Task> action, int delayMs = 2_000, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            await action().ConfigureAwait(false);
        }, cancellationToken);
    }
    public static Task<T> DoDelayedBackgroundAsync<T>(Func<Task<T>> action, int delayMs = 2_000, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            return await action().ConfigureAwait(false);
        }, cancellationToken);
    }
    
    public static T DoUntilNotDefault<T>(Func<T> action, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        return DoUntilCondition(
            action,
            result => !EqualityComparer<T?>.Default.Equals(result, default),
            timeMs,
            sleepMs,
            cancellationToken);
    }
    public static void DoUntilTimeEnd(Action action, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        DoUntilCondition(action, static () => false, timeMs, sleepMs, cancellationToken);
    }
    public static void DoUntilCondition(Action action, Func<bool> predicate, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            action();
            if (predicate())
                return;

            Thread.Sleep(sleepMs);
        }
    }
    public static T DoUntilCondition<T>(Func<T> action, Predicate<T> predicate, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            var result = action();
            if (predicate(result))
                return result;

            Thread.Sleep(sleepMs);
        }

        return action();
    }
    public static void DoIfCondition(Action action, Func<bool> predicate, bool justOnce = false, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            if (predicate())
            {
                action();

                if (justOnce) return;
            }
            Thread.Sleep(sleepMs);
        }
    }
    public static Task<T> DoUntilNotDefaultAsync<T>(Func<Task<T>> action, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        return DoUntilConditionAsync(
            action,
            result => !EqualityComparer<T?>.Default.Equals(result, default),
            timeMs,
            sleepMs,
            cancellationToken);
    }
    public static Task<T> DoUntilNotDefaultAsync<T>(Func<T> action, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        return DoUntilConditionAsync(
            action,
            result => !EqualityComparer<T?>.Default.Equals(result, default),
            timeMs,
            sleepMs,
            cancellationToken);
    }
    public static Task DoUntilTimeEndAsync(Func<Task> action, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        return DoUntilConditionAsync(action, static () => false, timeMs, sleepMs, cancellationToken);
    }
    public static async Task DoUntilConditionAsync(Func<Task> action, Func<bool> predicate, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            await action().ConfigureAwait(false);
            if (predicate())
                return;

            await Task.Delay(sleepMs).ConfigureAwait(false);
        }
    }
    public static async Task<T> DoUntilConditionAsync<T>(Func<T> action, Predicate<T> predicate, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            var result = action();
            if (predicate(result))
                return result;

            await Task.Delay(sleepMs).ConfigureAwait(false);
        }

        return action();
    }
    public static async Task<T> DoUntilConditionAsync<T>(Func<Task<T>> action, Predicate<T> predicate, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            var result = await action().ConfigureAwait(false);
            if (predicate(result))
                return result;

            await Task.Delay(sleepMs).ConfigureAwait(false);
        }

        return await action().ConfigureAwait(false);
    }
    public static async Task DoIfConditionAsync(Func<Task> action, Func<bool> predicate, bool justOnce = false, int timeMs = 500, int sleepMs = 20, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            if (predicate())
            {
                await action().ConfigureAwait(false);

                if (justOnce) return;
            }
            await Task.Delay(sleepMs).ConfigureAwait(false);
        }
    }

    public static bool IsTimeUp(long startTicks, int timeMs)
    {

#if NET7_0_OR_GREATER
        var elapsedTime = Stopwatch.GetElapsedTime(startTicks);
#else
        var elapsedTime = GetElapsedTime(startTicks);
#endif

        return elapsedTime.TotalMilliseconds >= timeMs;
    }
    public static TimeSpan GetElapsedTime(long startTicks)
    {
        var tickFrequency = (double)10_000_000 / Stopwatch.Frequency;
        return new TimeSpan((long)((Stopwatch.GetTimestamp() - startTicks) * tickFrequency));
    }
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        if (val.CompareTo(max) > 0) return max;
        return val;
    }
    
    public static Icon? GetIcon() => Icon.ExtractAssociatedIcon(GetExecutablePath());

    public static bool IsExplorerEmptySpace(Point point)
    {
        var hr = WinApi.AccessibleObjectFromPoint(point, out var accObj, out var childId);
        if (hr != 0 || childId is not 0) return false;

        var role = accObj.get_accRole(0);
        return role is 0x21; //IAccessible.Role:list (ROLE_SYSTEM_LIST 0x21)
    }
    public static bool IsFileExplorerTab(nint tab)
    {
        return tab != 0 && WinApi.IsWindowHasClassName(tab, "ShellTabWindowClass");
    }
    public static bool IsFileExplorerWindow(nint window)
    {
        return window != 0 && WinApi.IsWindowHasClassName(window, "CabinetWClass");
    }
    public static bool IsFileExplorerForeground(out nint foregroundWindow)
    {
        foregroundWindow = WinApi.GetForegroundWindow();
        return IsFileExplorerWindow(foregroundWindow);
    }
    public static nint GetAnotherExplorerWindow(nint currentWindow)
    {
        return currentWindow == default
            ? WinApi.FindWindow("CabinetWClass", null)
            : WinApi.FindAllWindowsEx("CabinetWClass")
                .FirstOrDefault(window => window != currentWindow);
    }
    public static nint ListenForNewExplorerTab(IReadOnlyCollection<nint> currentTabs, int searchTimeMs = 1000)
    {
        return DoUntilNotDefault(() =>
                GetAllExplorerTabs()
                    .Except(currentTabs)
                    .FirstOrDefault(),
            searchTimeMs);
    }
    public static Task<nint> ListenForNewExplorerTabAsync(IReadOnlyCollection<nint> currentTabs, int searchTimeMs = 1000)
    {
        return DoUntilNotDefaultAsync(() =>
                GetAllExplorerTabs()
                    .Except(currentTabs)
                    .FirstOrDefault(),
            searchTimeMs);
    }
    public static Task<nint> ListenForNewExplorerTabAsync(nint window, IReadOnlyCollection<nint> currentTabs, int searchTimeMs = 1000)
    {
        return DoUntilNotDefaultAsync(() =>
                GetAllExplorerTabs(window)
                    .Except(currentTabs)
                    .FirstOrDefault(),
            searchTimeMs);
    }
    public static List<nint> GetAllExplorerTabs()
    {
        var tabs = new List<nint>();

        foreach (var window in WinApi.FindAllWindowsEx("CabinetWClass"))
            tabs.AddRange(WinApi.FindAllWindowsEx("ShellTabWindowClass", window));

        return tabs;
    }
    public static IEnumerable<nint> GetAllExplorerTabs(nint window)
    {
        return WinApi.FindAllWindowsEx("ShellTabWindowClass", window);
    }

    public static string GetExecutablePath()
    {
        var processName = Process.GetCurrentProcess().MainModule?.FileName;
        return processName is { Length: > 0 } ? processName : $"{AppDomain.CurrentDomain.FriendlyName}.exe";
    }
}