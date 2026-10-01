# Navigation completion regression checks

Run with the .NET 8 SDK:

```sh
dotnet run --project tests/NavigationCompletion.Tests.csproj --configuration Release
```

This dependency-free executable links the production navigation-completion helper.
It exits nonzero on a failed assertion. It covers synchronous and asynchronous
completion, timeout, synchronous and asynchronous navigation failure, duplicate
and late notifications, cleanup outside the callback thread's call stack, and
repeated navigation without retained subscriptions.

These checks do not automate Explorer or prove that the busy-cursor report in
issue #104 is resolved. Windows integration testing is still needed: open folders
into tabs, duplicate a tab with selected files, close a tab during navigation,
and verify selection, foreground activation, tab reuse, and cursor behavior.

The timeout bounds waiting for the completion event after the navigation call
returns; it cannot interrupt a hung synchronous COM call.
