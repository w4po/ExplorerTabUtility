using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ExplorerTabUtility.Interop;
using ExplorerTabUtility.Managers;
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
    public static string GetEnumDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        return fieldInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
    }

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

    public static string NormalizeLocation(string location)
    {
        if (location.IndexOf('%') > -1)
            location = Environment.ExpandEnvironmentVariables(location);

        if (location.StartsWith("::", StringComparison.Ordinal))
            location = $"shell:{location}";

        else if (location.StartsWith("{", StringComparison.Ordinal))
            location = $"shell:::{location}";

        location = location.Trim(' ', '/', '\\', '\n', '\'', '"');

        return location.Replace('/', '\\');
    }
    public static string GetDefaultExplorerLocation(ShellPathComparer? shellPathComparer = null)
    {
        var id = RegistryManager.GetDefaultExplorerLaunchId();
        var location = id switch
        {
            2 => "shell:::{F874310E-B6B7-47DC-BC84-B9E6B38F5903}",// Home, Quick Access
            3 => "shell:::{088E3905-0323-4B02-9826-5D99428E115F}",// Downloads
            4 => "shell:::{018D5C66-4533-4307-9B53-224DE2ED1FE6}",// OneDrive
            _ => "shell:::{20D04FE0-3AEA-1069-A2D8-08002B30309D}" // This PC
        };

        if (shellPathComparer == null)
            return location;

        var pidl = shellPathComparer.GetPidlFromPath(location);
        var path = ShellPathComparer.GetPathFromPidl(pidl); //SIGDN_URL: Downloads -> file:///C:/Users/Username/Downloads
        Marshal.FreeCoTaskMem(pidl);

        return NormalizeLocation(path ?? location);
    }

    public static string GetExecutablePath()
    {
        var processName = Process.GetCurrentProcess().MainModule?.FileName;
        return processName is { Length: > 0 } ? processName : $"{AppDomain.CurrentDomain.FriendlyName}.exe";
    }
}