using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace ExplorerTabUtility.Helpers;

public static class Helper
{
    public static T DoUntilNotDefault<T>(Func<T> action, int timeMs = 500, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            var result = action.Invoke();
            if (!EqualityComparer<T?>.Default.Equals(result, default))
                return result;
        }

        return action.Invoke();
    }
    public static void DoUntilTimeEnd(Action action, int timeMs = 5_000, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();

        while (!cancellationToken.IsCancellationRequested && !IsTimeUp(startTicks, timeMs))
        {
            action.Invoke();
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

    public static Icon? GetIcon()
    {
        var processName = Process.GetCurrentProcess().MainModule?.FileName;

        var location = string.IsNullOrWhiteSpace(processName)
            ? $"{AppDomain.CurrentDomain.FriendlyName}.exe"
            : processName;

        return Icon.ExtractAssociatedIcon(location);
    }
}