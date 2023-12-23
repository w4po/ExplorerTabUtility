using System;

namespace ExplorerTabUtility.Hooks;

public interface IHook : IDisposable
{
    public bool IsHookActive { get; }
    public void StartHook();
    public void StopHook();
}