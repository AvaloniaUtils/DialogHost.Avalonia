using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using DialogHostAvalonia.Positioners;

namespace DialogHostAvalonia.DialogControls {
    public class PositionedContentControl : ContentControl {
        public static readonly StyledProperty<IDialogPopupPositioner> PositionerProperty = 
            AvaloniaProperty.Register<PositionedContentControl, IDialogPopupPositioner>(nameof(Positioner));

        public IDialogPopupPositioner Positioner {
            get { return GetValue(PositionerProperty); }
            set { SetValue(PositionerProperty, value); }
        }

        /// <inheritdoc />
        protected override void ArrangeCore(Rect finalRect) {
            var margin = Margin;

            var useLayoutRounding = UseLayoutRounding;
            var scale = LayoutHelper.GetLayoutScale(this);
            if (useLayoutRounding)
            {
                margin = LayoutHelper.RoundLayoutThickness(margin, scale, scale);
            }

            var availableSizeMinusMargins = new Size(
                Math.Max(0, finalRect.Width - margin.Left - margin.Right),
                Math.Max(0, finalRect.Height - margin.Top - margin.Bottom));
            Positioner.Arrange(DesiredSize, availableSizeMinusMargins, scale);
        }
    }
}