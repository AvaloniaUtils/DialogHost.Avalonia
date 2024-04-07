using System;

namespace DialogHostAvalonia.Utilities;

/// <summary>
/// Represents a disposable that does nothing on disposal.
/// </summary>
internal sealed class EmptyDisposable : IDisposable
{
    public static readonly EmptyDisposable Instance = new();

    private EmptyDisposable()
    {
    }

    public void Dispose()
    {
        // no op
    }
}