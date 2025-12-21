using System;
using Avalonia.Interactivity;

namespace DialogHostAvalonia;

/// <summary>
/// Event args contains info about dialog closing
/// </summary>
public class DialogClosingEventArgs(DialogSession session, RoutedEvent routedEvent, bool canBeCancelled)
    : RoutedEventArgs(routedEvent) {
    /// <inheritdoc />
    [Obsolete("Use constructor with canBeCancelled parameter")]
    public DialogClosingEventArgs(DialogSession session, RoutedEvent routedEvent) : this(session, routedEvent, true) {
    }

    /// <summary>
    /// Cancel the close.
    /// </summary>
    public void Cancel() {
        if (!CanBeCancelled) {
            throw new InvalidOperationException(
                $"Cannot cancel dialog closing after {nameof(DialogHost)}.{nameof(DialogHost.IsOpen)} " +
                $"property has been set to {bool.FalseString}");
        }

        IsCancelled = true;
    }

    /// <summary>
    /// Indicates if the close has already been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Gets the parameter originally provided to <see cref="DialogHost.CloseDialogCommand"/>/
    /// </summary>
    public object? Parameter => Session.CloseParameter;

    /// <summary>
    /// Allows interaction with the current dialog session.
    /// </summary>
    public DialogSession Session { get; } = session ?? throw new ArgumentNullException(nameof(session));

    /// <summary>
    /// Indicates if the close can be canceled.
    /// </summary>
    /// <remarks>
    /// Typically this is <c>true</c> unless closing is triggered by setting <see cref="DialogHost.IsOpen"/> to <c>false</c>.
    /// </remarks>
    public bool CanBeCancelled { get; } = canBeCancelled;
}