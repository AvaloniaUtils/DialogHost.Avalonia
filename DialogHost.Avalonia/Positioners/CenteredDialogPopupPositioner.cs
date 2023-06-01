using Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace DialogHostAvalonia.Positioners {
    /// <summary>
    /// Positions the popup at the screen center
    /// </summary>
    public class CenteredDialogPopupPositioner : IDialogPopupPositioner {
        public static CenteredDialogPopupPositioner Instance { get; } = new();
        
        /// <inheritdoc />
        public Rect Update(Size anchorRectangle, Size size) {
            // Simplify calculations
            var horizontalMargin = (anchorRectangle.Width - size.Width) / 2;
            var verticalMargin = (anchorRectangle.Height - size.Height) / 2;
            return new Rect(new Point(horizontalMargin, verticalMargin), size);
        }
    }
}