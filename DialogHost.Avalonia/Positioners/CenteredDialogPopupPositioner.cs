using System;
using Avalonia;

namespace DialogHostAvalonia.Positioners {
    /// <summary>
    /// Positions the popup at the screen center
    /// </summary>
    public class CenteredDialogPopupPositioner : IDialogPopupPositioner {
        /// <summary>
        /// Singleton instance of <see cref="CenteredDialogPopupPositioner"/>
        /// </summary>
        public static CenteredDialogPopupPositioner Instance { get; } = new();
        
        /// <inheritdoc />
        public Rect Arrange(Size size, Size availableSize, double scale) {
            var horizontalMargin = (availableSize.Width - size.Width) / 2;
            var verticalMargin = (availableSize.Height - size.Height) / 2;
            return new Rect(horizontalMargin / scale, verticalMargin / scale, size.Width / scale, size.Height / scale);
        }

        /// <inheritdoc />
        public event EventHandler? RearrangeRequested;
    }
}