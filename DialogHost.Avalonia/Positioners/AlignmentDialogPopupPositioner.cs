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
        /// <summary>
        /// Defines the <see cref="HorizontalAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty
            = Layoutable.HorizontalAlignmentProperty.AddOwner<AlignmentDialogPopupPositioner>();

        /// <summary>
        /// Defines the <see cref="VerticalAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentProperty
            = Layoutable.VerticalAlignmentProperty.AddOwner<AlignmentDialogPopupPositioner>();

        /// <summary>
        /// Defines the <see cref="Margin"/> property.
        /// </summary>
        public static readonly StyledProperty<Thickness> MarginProperty
            = Layoutable.MarginProperty.AddOwner<AlignmentDialogPopupPositioner>();

        static AlignmentDialogPopupPositioner() {
            HorizontalAlignmentProperty.Changed.AddClassHandler<AlignmentDialogPopupPositioner>((x, _) => x.OnRearrangeRequested());
            VerticalAlignmentProperty.Changed.AddClassHandler<AlignmentDialogPopupPositioner>((x, _) => x.OnRearrangeRequested());
            MarginProperty.Changed.AddClassHandler<AlignmentDialogPopupPositioner>((x, _) => x.OnRearrangeRequested());
        }

        /// <summary>
        /// Gets or sets the element's preferred horizontal alignment in its parent.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment {
            get => GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the element's preferred vertical alignment in its parent.
        /// </summary>
        public VerticalAlignment VerticalAlignment {
            get => GetValue(VerticalAlignmentProperty);
            set => SetValue(VerticalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin around the element.
        /// </summary>
        public Thickness Margin {
            get => GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        /// <inheritdoc />
        public Rect Arrange(Size size, Size availableSize, double scale) {
            var margin = Margin;

            var constrainRect = new Rect(availableSize).Deflate(margin);
            var aligned = new Rect(size).Align(constrainRect, HorizontalAlignment, VerticalAlignment);
            var final = new Rect(margin.Left + aligned.Left, margin.Top + aligned.Top, aligned.Width, aligned.Height);
            return new Rect(final.Position / scale, final.Size / scale);
        }

        /// <inheritdoc />
        public event EventHandler? RearrangeRequested;

        /// <summary>
        /// Invokes the <see cref="RearrangeRequested"/>
        /// </summary>
        protected virtual void OnRearrangeRequested() {
            RearrangeRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}