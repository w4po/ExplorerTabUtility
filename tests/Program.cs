using System;
using System.Threading.Tasks;
using ExplorerTabUtility.Helpers;

static class Program
{
    [ThreadStatic] private static bool _insideCallback;

    static async Task Main()
    {
        foreach (var mode in new[] { "success", "timeout", "failure", "duplicate" })
        {
            Action handler = null;
            int attached = 0, detached = 0;
            bool result = false, failed = false;
            try
            {
                result = await NavigationCompletion.WaitAsync(
                    h => { handler = h; attached++; },
                    h => { if (handler != h) throw new Exception("Wrong subscription"); handler = null; detached++; },
                    () =>
                    {
                        if (mode == "failure") throw new InvalidOperationException("Simulated navigation failure");
                        if (mode == "success" || mode == "duplicate") handler();
                        if (mode == "duplicate") handler();
                        return Task.CompletedTask;
                    }, 20);
            }
            catch (InvalidOperationException) { failed = true; }
            if (attached != 1 || detached != 1 || handler != null ||
                failed != (mode == "failure") || result != (mode == "success" || mode == "duplicate"))
                throw new Exception("FAIL: " + mode);
            Console.WriteLine("PASS: " + mode + "; subscription removed exactly once");
        }

        // An asynchronous event must not run the await continuation (and hence
        // COM unsubscription/selection) inline on Explorer's callback thread.
        Action signal = null;
        var pending = NavigationCompletion.WaitAsync(
            h => signal = h,
            _ => { if (_insideCallback) throw new Exception("Inline callback cleanup"); },
            () => Task.CompletedTask);
        _insideCallback = true;
        try { signal(); }
        finally { _insideCallback = false; }
        if (!await pending) throw new Exception("Asynchronous event was lost");
        Console.WriteLine("PASS: asynchronous completion does not clean up inline on callback thread");

        Action lateEvent = null;
        int lateDetachCount = 0;
        var timedOut = NavigationCompletion.WaitAsync(
            h => lateEvent = h, _ => lateDetachCount++, () => Task.CompletedTask, 20);
        if (await timedOut) throw new Exception("Expected timeout");
        lateEvent(); // Simulate a notification already in flight when detached.
        if (await timedOut || lateDetachCount != 1) throw new Exception("Late event changed the result");
        Console.WriteLine("PASS: late notification cannot revive timed-out navigation");

        int faultDetachCount = 0;
        try
        {
            await NavigationCompletion.WaitAsync(_ => { }, _ => faultDetachCount++,
                () => Task.FromException(new InvalidOperationException("Asynchronous failure")));
            throw new Exception("Expected asynchronous navigation failure");
        }
        catch (InvalidOperationException)
        {
            if (faultDetachCount != 1) throw new Exception("Faulted task retained subscription");
        }
        Console.WriteLine("PASS: faulted navigation task removes subscription");

        int activeSubscriptions = 0;
        for (int i = 0; i < 100; i++)
        {
            Action completed = null;
            await NavigationCompletion.WaitAsync(
                h => { completed = h; activeSubscriptions++; },
                _ => activeSubscriptions--,
                () => { completed(); return Task.CompletedTask; });
        }
        if (activeSubscriptions != 0) throw new Exception("Repeated navigation leaked subscriptions");
        Console.WriteLine("PASS: 100 repeated navigations leave no subscriptions");
    }
}
