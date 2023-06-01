using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DialogHostAvalonia.Positioners;

namespace DialogHostAvalonia {
    public class DialogOverlayPopupHost : ContentControl, ICustomKeyboardNavigation {
        public static readonly DirectProperty<DialogOverlayPopupHost, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<DialogOverlayPopupHost, bool>(
                nameof(IsOpen),
                o => o.IsOpen,
                (o, v) => o.IsOpen = v);

        public static readonly StyledProperty<bool> IsActuallyOpenProperty =
            AvaloniaProperty.Register<DialogOverlayPopupHost, bool>(nameof(IsActuallyOpen), true);

        public static readonly DirectProperty<DialogOverlayPopupHost, bool> DisableOpeningAnimationProperty =
            DialogHost.DisableOpeningAnimationProperty.AddOwner<DialogOverlayPopupHost>(
                host => host.DisableOpeningAnimation,
                (host, b) => host.DisableOpeningAnimation = b);

        public static readonly DirectProperty<DialogOverlayPopupHost, IDialogPopupPositioner?> PopupPositionerProperty =
            DialogHost.PopupPositionerProperty.AddOwner<DialogOverlayPopupHost>(
                o => o.PopupPositioner,
                (o, v) => o.PopupPositioner = v);

        private readonly Grid _root;

        private bool _disableOpeningAnimation;
        private bool _isOpen;
        private IDialogPopupPositioner? _popupPositioner;

        public DialogOverlayPopupHost(Grid root)
        {
            this._root = root;
        }

        public bool IsOpen {
            get => _isOpen;
            set {
                SetAndRaise(IsOpenProperty, ref _isOpen, value);
                if (value) Show();
            }
        }

        /// <summary>
        /// Controls <see cref="Show"/> and <see cref="Hide"/> calls. Used for closing animations
        /// </summary>
        /// <remarks>
        /// Actually you should use <see cref="IsOpen"/> for opening and closing dialog
        /// </remarks>
        public bool IsActuallyOpen {
            get => GetValue(IsActuallyOpenProperty);
            set => SetValue(IsActuallyOpenProperty, value);
        }

        public bool DisableOpeningAnimation {
            get => _disableOpeningAnimation;
            set => SetAndRaise(DisableOpeningAnimationProperty, ref _disableOpeningAnimation, value);
        }

        public IDialogPopupPositioner? PopupPositioner {
            get => _popupPositioner;
            set {
                SetAndRaise(PopupPositionerProperty, ref _popupPositioner, value);
                UpdatePosition();
            }
        }

        public void Show()
        {
            if (Parent == null) {
                _root.Children.Add(this);
            }
            // Set the minimum priority to allow overriding it everywhere
            ClearValue(IsActuallyOpenProperty);
            Focus();
            UpdatePosition();
        }

        public void Hide()
        {
            _root.Children.Remove(this);
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
            var positioner = PopupPositioner ?? CenteredDialogPopupPositioner.Instance;
            var bounds = positioner.Update(size, contentSize);
            
            ArrangeOverride(bounds.Size).Constrain(size);
            Bounds = new Rect(bounds.X + margin.Left, bounds.Y + margin.Top, bounds.Width, bounds.Height);
        }


        private void UpdatePosition()
        {
            // Don't bother the positioner with layout system artifacts
            // if (_positionerParameters.Size.Width == 0 || _positionerParameters.Size.Height == 0)
                // return;
            // if (Parent != null)
            // {
                // _popupPositioner.Update(_positionerParameters.);
            // }
        }

        public (bool handled, IInputElement? next) GetNext(IInputElement element, NavigationDirection direction) {
            if (!element.Equals(this)) {
                return (false, null);
            }
            var focusable = this.GetVisualDescendants().OfType<IInputElement>().FirstOrDefault(visual => visual.Focusable);
            return (true, focusable);
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            if (change.Property == IsActuallyOpenProperty && !change.GetNewValue<bool>()) {
                Hide();
            }
            base.OnPropertyChanged(change);
        }
    }
}