using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using DialogHostAvalonia.Positioners;

namespace DialogHostAvalonia.DialogControls {
    public class PositionedContentControl : ContentControl {
        static PositionedContentControl() {
            AffectsArrange<PositionedContentControl>(PositionerProperty);
            PositionerProperty.Changed.AddClassHandler<PositionedContentControl>((control, args) => {
                var (oldValue, newValue) = args.GetOldAndNewValue<IDialogPopupPositioner?>();
                if (oldValue != null) {
                    oldValue.RearrangeRequested -= control.OnRearrangeRequested;
                }
                if (newValue != null) {
                    newValue.RearrangeRequested += control.OnRearrangeRequested;
                }
            });
        }

        public static readonly StyledProperty<IDialogPopupPositioner?> PositionerProperty = 
            AvaloniaProperty.Register<PositionedContentControl, IDialogPopupPositioner?>(nameof(Positioner));

        public IDialogPopupPositioner? Positioner {
            get => GetValue(PositionerProperty);
            set => SetValue(PositionerProperty, value);
        }
        
        private void OnRearrangeRequested(object sender, EventArgs e) {
            InvalidateArrange();
        }

        /// <inheritdoc />
        protected override void ArrangeCore(Rect finalRect) {
            var margin = Margin;

            var size = new Size(
                Math.Max(0, finalRect.Width - margin.Left - margin.Right),
                Math.Max(0, finalRect.Height - margin.Top - margin.Bottom));
            
            var contentSize = new Size(
                Math.Min(size.Width, DesiredSize.Width - margin.Left - margin.Right), 
                Math.Min(size.Height, DesiredSize.Height - margin.Top - margin.Bottom));
            var positioner = Positioner ?? CenteredDialogPopupPositioner.Instance;
            var bounds = positioner.Arrange(contentSize, size, 1);
            
            ArrangeOverride(bounds.Size).Constrain(size);
            Bounds = new Rect(bounds.X + margin.Left, bounds.Y + margin.Top, bounds.Width, bounds.Height);
        }
    }
}