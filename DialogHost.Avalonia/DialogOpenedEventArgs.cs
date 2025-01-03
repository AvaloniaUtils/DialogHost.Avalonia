using System;
using Avalonia.Interactivity;

namespace DialogHostAvalonia;

/// <summary>
/// Event args contains info about dialog opening
/// </summary>
public class DialogOpenedEventArgs(DialogSession session, RoutedEvent routedEvent) : RoutedEventArgs(routedEvent) {
    /// <summary>
    /// Allows interaction with the current dialog session.
    /// </summary>
    public DialogSession Session { get; } = session;
}