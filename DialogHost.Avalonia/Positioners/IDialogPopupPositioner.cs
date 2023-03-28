using System;
using Avalonia;

namespace DialogHostAvalonia.Positioners {
    /// <summary>
    /// Allows you to define custom positioners for your <see cref="DialogHost"/>'s popups
    /// </summary>
    public interface IDialogPopupPositioner {
        /// <summary>
        /// Called when we need to determine popup location (after opening, window resizing, etc)
        /// </summary>
        /// <returns>Rectangle with control bounds</returns>
        public Rect Arrange(Size size, Size availableSize, double scale);
        
        /// <summary>
        /// Event, which triggered when positioner need to rearrange a popup
        /// </summary>
        public event EventHandler? RearrangeRequested;
    }
}