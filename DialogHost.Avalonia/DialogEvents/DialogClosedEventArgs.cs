using System;
using Avalonia.Interactivity;

namespace DialogHostAvalonia.DialogEvents {
    /// <summary>
    /// Arguments for <see cref="DialogHost.OnDialogClosing"/>
    /// </summary>
    public class DialogClosedEventArgs : RoutedEventArgs {
        /// <inheritdoc />
        public DialogClosedEventArgs(DialogSession session, RoutedEvent routedEvent)
            : base(routedEvent)
            => Session = session ?? throw new ArgumentNullException(nameof(session));

        /// <summary>
        /// Gets the parameter originally provided to <see cref="DialogHost.CloseDialogCommand"/>/
        /// </summary>
        public object? Parameter => Session.CloseParameter;

        /// <summary>
        /// Allows interaction with the current dialog session.
        /// </summary>
        public DialogSession Session { get; }
    }
}