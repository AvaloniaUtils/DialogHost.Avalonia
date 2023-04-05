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
    public class DialogOverlayPopupHost : ContentControl, IPopupHost, IManagedPopupPositionerPopup, ICustomKeyboardNavigation {
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

        private readonly OverlayLayer _overlayLayer;

        private bool _disableOpeningAnimation;
        private bool _isOpen;
        private Point _lastRequestedPosition;
        private DialogPopupPositionerHost _popupPositionerHost;
        private IDialogPopupPositioner? _popupPositioner;
        private PopupPositionerParameters _positionerParameters = new();
        private bool _shown;

        public DialogOverlayPopupHost(OverlayLayer overlayLayer)
        {
            _overlayLayer = overlayLayer;
            _popupPositionerHost = new DialogPopupPositionerHost(this, _popupPositioner);
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
                _popupPositionerHost._dialogPopupPositioner = value;
                UpdatePosition();
            }
        }

        IReadOnlyList<ManagedPopupPositionerScreenInfo> IManagedPopupPositionerPopup.Screens
        {
            get
            {
                var rc = new Rect(default, _overlayLayer.AvailableSize);
                return new[] {new ManagedPopupPositionerScreenInfo(rc, rc)};
            }
        }

        Rect IManagedPopupPositionerPopup.ParentClientAreaScreenGeometry =>
            new Rect(default, _overlayLayer.Bounds.Size);

        // TODO: Allow manipulation of the popup size
        void IManagedPopupPositionerPopup.MoveAndResize(Point devicePoint, Size virtualSize)
        {
            _lastRequestedPosition = devicePoint;
            Dispatcher.UIThread.Post(() =>
            {
                Canvas.SetLeft(this, _lastRequestedPosition.X);
                Canvas.SetTop(this, _lastRequestedPosition.Y);
                Width = virtualSize.Width;
                Height = virtualSize.Height;
            }, DispatcherPriority.Layout);
        }

        double IManagedPopupPositionerPopup.Scaling => 1;

        public void SetChild(Control? control)
        {
            Content = control;
        }

        bool IPopupHost.Topmost {
            get => false;
            set { /* Not supported */ }
        }

        Transform? IPopupHost.Transform { get; set; }
        public Visual? HostedVisualTreeRoot => null;

        public void Dispose() => Hide();


        public void Show()
        {
            if (!_shown) {
                _overlayLayer.Children.Add(this);
            }
            _shown = true;
            // Set the minimum priority to allow overriding it everywhere
            SetValue(IsActuallyOpenProperty, true, BindingPriority.Style);
            Focus();
            UpdatePosition();
        }

        public void Hide()
        {
            _overlayLayer.Children.Remove(this);
            _shown = false;
        }

        public void ConfigurePosition(Visual target, PlacementMode placement, Point offset,
                                      PopupAnchor anchor = PopupAnchor.None, PopupGravity gravity = PopupGravity.None,
                                      PopupPositionerConstraintAdjustment constraintAdjustment = PopupPositionerConstraintAdjustment.All,
                                      Rect? rect = null)
        {
            // This code handles only PlacementMode.AnchorAndGravity and other default values
            // Suitable only for current implementation of DialogHost
            _positionerParameters.AnchorRectangle = target.Bounds;

            UpdatePosition();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_positionerParameters.Size != finalSize)
            {
                _positionerParameters.Size = finalSize;
                UpdatePosition();
            }
            return base.ArrangeOverride(finalSize);
        }


        private void UpdatePosition()
        {
            // Don't bother the positioner with layout system artifacts
            if (_positionerParameters.Size.Width == 0 || _positionerParameters.Size.Height == 0)
                return;
            if (_shown)
            {
                _popupPositionerHost.Update(_positionerParameters);
            }
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