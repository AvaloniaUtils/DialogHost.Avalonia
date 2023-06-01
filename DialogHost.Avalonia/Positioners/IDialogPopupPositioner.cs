using Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace DialogHostAvalonia.Positioners {
    /// <summary>
    /// Allows you to define custom positioners for your <see cref="DialogHost"/>'s popups
    /// </summary>
    public interface IDialogPopupPositioner {
        /// <summary>
        /// Called when we need to determine popup location (after opening, window resizing, etc)
        /// </summary>
        /// <remarks>
        /// Use <see cref="IManagedPopupPositionerPopup.MoveAndResize"/> to manipulate popup
        /// </remarks>
        /// <param name="parameters">Parameters for popup</param>
        /// <param name="anchorRectangle">Owner's rectangle size</param>
        /// <param name="size">Size of popup</param>
        public Rect Update(Size anchorRectangle, Size size);
    }
}