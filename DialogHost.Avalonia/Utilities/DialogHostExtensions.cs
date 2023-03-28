using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DialogHostAvalonia.DialogEvents;

namespace DialogHostAvalonia.Utilities {
    /// <summary>
    /// Set of extension methods for <see cref="DialogHost"/>
    /// </summary>
    public static class DialogHostExtensions {
        /// <summary>
        /// Shows a dialog using the first found <see cref="DialogHost"/> in a given <see cref="Window"/>.
        /// </summary>
        /// <param name="window">Window on which the modal dialog should be displayed. Must contain a <see cref="DialogHost"/>.</param>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closedEventHandler">Allows access to closed event which would otherwise have been subscribed to on a instance.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown is a <see cref="DialogHost"/> is not found when conducting a depth first traversal of visual tree.  
        /// </exception>
        /// <remarks>
        /// As a depth first traversal of the window's visual tree is performed, it is not safe to use this method in a situation where a screen has multiple <see cref="DialogHost"/>s.
        /// </remarks>
        /// <returns></returns>
        public static Task<object?> ShowDialogViaDialogHost(this Window window, object content, 
                                                            DialogOpenedEventHandler? openedEventHandler = null,
                                                            DialogClosingEventHandler? closingEventHandler = null,
                                                            DialogClosedEventHandler? closedEventHandler = null)
            => GetFirstDialogHost(window).ShowInternal(content, openedEventHandler, closingEventHandler, closedEventHandler);

        /// <summary>
        /// Shows a dialog using the parent/ancestor <see cref="DialogHost"/> of the a given <see cref="Visual"/>.
        /// </summary>
        /// <param name="childDependencyObject">Dependency object which should be a visual child of a <see cref="DialogHost"/>, where the dialog will be shown.</param>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closedEventHandler">Allows access to closed event which would otherwise have been subscribed to on a instance.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown is a <see cref="DialogHost"/> is not found when conducting a depth first traversal of visual tree.  
        /// </exception>
        /// <returns></returns>
        public static Task<object?> ShowDialogViaDialogHost(this Visual childDependencyObject, object content, 
                                                            DialogOpenedEventHandler? openedEventHandler = null,
                                                            DialogClosingEventHandler? closingEventHandler = null,
                                                            DialogClosedEventHandler? closedEventHandler = null)
            => GetOwningDialogHost(childDependencyObject).ShowInternal(content, openedEventHandler, closingEventHandler, closedEventHandler);

        private static DialogHost GetFirstDialogHost(Window window)
        {
            if (window is null) throw new ArgumentNullException(nameof(window));

            var dialogHost = window.FindDescendantOfType<DialogHost>();

            if (dialogHost is null)
                throw new InvalidOperationException("Unable to find a DialogHost in visual tree");

            return dialogHost;
        }

        private static DialogHost GetOwningDialogHost(Visual childDependencyObject)
        {
            if (childDependencyObject is null) throw new ArgumentNullException(nameof(childDependencyObject));

            var dialogHost = childDependencyObject.FindAncestorOfType<DialogHost>();

            if (dialogHost is null)
                throw new InvalidOperationException("Unable to find a DialogHost in visual tree ancestry");

            return dialogHost;
        }
    }
}