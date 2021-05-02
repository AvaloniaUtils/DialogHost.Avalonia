using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace DialogHost {
    public class DialogOverlayPopupHost : OverlayPopupHost {
        public static readonly DirectProperty<DialogOverlayPopupHost, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<DialogOverlayPopupHost, bool>(
                nameof(IsOpen),
                o => o.IsOpen,
                (o, v) => o.IsOpen = v);

        public static readonly DirectProperty<DialogOverlayPopupHost, bool> InternalIsShownProperty =
            AvaloniaProperty.RegisterDirect<DialogOverlayPopupHost, bool>(
                nameof(InternalIsShown),
                o => o.InternalIsShown,
                (o, v) => o.InternalIsShown = v);

        private bool _internalIsShown;

        private bool _isOpen;

        public DialogOverlayPopupHost(OverlayLayer overlayLayer) : base(overlayLayer) { }

        public bool IsOpen {
            get => _isOpen;
            set {
                SetAndRaise(IsOpenProperty, ref _isOpen, value);
                if (value) InternalIsShown = true;
            }
        }

        /// <summary>
        /// Controls <see cref="Show"/> and <see cref="Hide"/> calls. Used for closing animations
        /// </summary>
        /// <remarks>
        /// Actually you should use <see cref="IsOpen"/> for opening and closing dialog 
        /// </remarks>
        public bool InternalIsShown {
            get { return _internalIsShown; }
            set {
                var previousValue = _internalIsShown;
                SetAndRaise(InternalIsShownProperty, ref _internalIsShown, value); 
                switch (previousValue) {
                    case true when !value:
                        base.Hide();
                        break;
                    case false when value:
                        base.Show();
                        break;
                }
            }
        }

        public new void Show()
        {
            throw new NotSupportedException("Use IsOpen property for showing dialog");
        }

        public new void Hide() {
            throw new NotSupportedException("Use IsOpen property for hiding dialog");
        }

        public override void Render(DrawingContext context) { }
    }
}