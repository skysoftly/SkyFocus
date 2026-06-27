using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace SkyFocus.Services;

public static class UiHelper
{
    public static async Task<T> InvokeAsync<T>(Func<Task<T>> action)
    {
        return await Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Input);
    }
}