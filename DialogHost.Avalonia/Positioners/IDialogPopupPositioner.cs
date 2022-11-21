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
        /// <param name="popup">The popup handler</param>
        /// <param name="parameters">Parameters for popup</param>
        public void Update(IManagedPopupPositionerPopup popup, PopupPositionerParameters parameters);
    }
}