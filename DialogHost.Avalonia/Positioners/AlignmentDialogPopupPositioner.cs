using System;
using Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Layout;

namespace DialogHostAvalonia.Positioners {
    /// <summary>
    /// Positions the popup according to <see cref="HorizontalAlignment"/>, <see cref="VerticalAlignment"/> and even <see cref="Margin"/>
    /// </summary>
    /// <remarks>
    /// Default values for <see cref="HorizontalAlignment"/> and <see cref="VerticalAlignment"/> is <c>Stretch</c> and it will be act TopLeft alignment
    /// </remarks>
    public class AlignmentDialogPopupPositioner : AvaloniaObject, IDialogPopupPositioner {
        public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty
            = Layoutable.HorizontalAlignmentProperty.AddOwner<AlignmentDialogPopupPositioner>();

        public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentProperty
            = Layoutable.VerticalAlignmentProperty.AddOwner<AlignmentDialogPopupPositioner>();

        public static readonly StyledProperty<Thickness> MarginProperty
            = Layoutable.MarginProperty.AddOwner<AlignmentDialogPopupPositioner>();

        public HorizontalAlignment HorizontalAlignment {
            get => GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        public VerticalAlignment VerticalAlignment {
            get => GetValue(VerticalAlignmentProperty);
            set => SetValue(VerticalAlignmentProperty, value);
        }

        public Thickness Margin {
            get => GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        // TODO: Changes in properties ^ should call this method 
        /// <inheritdoc />
        public void Update(IManagedPopupPositionerPopup popup, PopupPositionerParameters parameters) {
            var margin = GetValue(MarginProperty);

            var availableSpaceRect = new Rect(parameters.AnchorRectangle.Size);
            var constrainRect = availableSpaceRect.Deflate(margin);
            var aligned = new Rect(parameters.Size).Align(constrainRect, GetValue(HorizontalAlignmentProperty), GetValue(VerticalAlignmentProperty));
            var final = new Rect(margin.Left + aligned.Left, margin.Top + aligned.Top, aligned.Width, aligned.Height);
            popup.MoveAndResize(final.Position / popup.Scaling, final.Size / popup.Scaling);
        }
    }
}