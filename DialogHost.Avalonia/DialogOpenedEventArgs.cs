using System;
using Avalonia.Interactivity;

namespace DialogHostAvalonia {
    /// <summary>
    /// Arguments for <see cref="DialogHost.OnDialogOpened"/>
    /// </summary>
    public class DialogOpenedEventArgs : RoutedEventArgs {
        /// <inheritdoc />
        public DialogOpenedEventArgs(DialogSession session, RoutedEvent routedEvent)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            Session = session;
            RoutedEvent = routedEvent;
        }

        /// <summary>
        /// Allows interation with the current dialog session.
        /// </summary>
        public DialogSession Session { get; }
    }
}