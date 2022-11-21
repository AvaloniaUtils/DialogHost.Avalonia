using Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace DialogHostAvalonia.Positioners {
    /// <summary>
    /// Positions the popup at the screen center
    /// </summary>
    public class CenteredDialogPopupPositioner : IDialogPopupPositioner {
        public static CenteredDialogPopupPositioner Instance { get; } = new();
        
        /// <inheritdoc />
        public void Update(IManagedPopupPositionerPopup popup, PopupPositionerParameters parameters) {
            // Simplify calculations
            var horizontalMargin = (parameters.AnchorRectangle.Width - parameters.Size.Width) / 2;
            var verticalMargin = (parameters.AnchorRectangle.Height - parameters.Size.Height) / 2;
            popup.MoveAndResize(new Point(horizontalMargin, verticalMargin), parameters.Size / popup.Scaling);
        }
    }
}