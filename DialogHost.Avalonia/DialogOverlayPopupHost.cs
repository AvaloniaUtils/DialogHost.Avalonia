using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Avalonia.DialogHost {
    public class DialogOverlayPopupHost : OverlayPopupHost {
        public DialogOverlayPopupHost(OverlayLayer overlayLayer) : base(overlayLayer) { }

        public override void Render(DrawingContext context) { }
    }
}