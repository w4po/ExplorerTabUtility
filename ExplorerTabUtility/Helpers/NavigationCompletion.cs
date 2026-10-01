using System;
using System.Threading.Tasks;

namespace ExplorerTabUtility.Helpers;

// The COM event must only signal completion, never call back into Explorer.
internal static class NavigationCompletion
{
    internal static async Task<bool> WaitAsync(
        Action<Action> subscribe, Action<Action> unsubscribe,
        Func<Task> navigate, int timeoutMilliseconds = 5000)
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Action completed = () => completion.TrySetResult(true);
        subscribe(completed);
        try
        {
            await navigate();
            var winner = await Task.WhenAny(completion.Task, Task.Delay(timeoutMilliseconds));
            return winner == completion.Task;
        }
        finally
        {
            // Also detach when navigation fails or no completion event arrives.
            unsubscribe(completed);
        }
    }
}
