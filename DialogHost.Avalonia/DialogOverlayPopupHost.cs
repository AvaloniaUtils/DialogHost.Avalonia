using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace DialogHost {
    public class DialogOverlayPopupHost : ContentControl, IPopupHost, IInteractive, IManagedPopupPositionerPopup {
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

        private readonly OverlayLayer _overlayLayer;

        private bool _internalIsShown;

        private bool _isOpen;
        private Point _lastRequestedPosition;
        private DialogPopupPositioner _popupPositioner;
        private PopupPositionerParameters _positionerParameters = new PopupPositionerParameters();
        private bool _shown;

        public DialogOverlayPopupHost(OverlayLayer overlayLayer)
        {
            _overlayLayer = overlayLayer;
            _popupPositioner = new DialogPopupPositioner(this);
        }

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
                        Hide();
                        break;
                    case false when value:
                        Show();
                        break;
                }
            }
        }

        /// <inheritdoc/>
        IInteractive IInteractive.InteractiveParent => Parent;

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

        void IManagedPopupPositionerPopup.MoveAndResize(Point devicePoint, Size virtualSize)
        {
            _lastRequestedPosition = devicePoint;
            Dispatcher.UIThread.Post(() =>
            {
                OverlayLayer.SetLeft(this, _lastRequestedPosition.X);
                OverlayLayer.SetTop(this, _lastRequestedPosition.Y);
            }, DispatcherPriority.Layout);
        }

        double IManagedPopupPositionerPopup.Scaling => 1;

        public void SetChild(IControl control)
        {
            Content = control;
        }

        public IVisual HostedVisualTreeRoot => null;

        public void Dispose() => Hide();


        public void Show()
        {
            _overlayLayer.Children.Add(this);
            _shown = true;
            UpdatePosition();
        }

        public void Hide()
        {
            _overlayLayer.Children.Remove(this);
            _shown = false;
        }

        public IDisposable BindConstraints(AvaloniaObject popup, StyledProperty<double> widthProperty, StyledProperty<double> minWidthProperty,
                                           StyledProperty<double> maxWidthProperty, StyledProperty<double> heightProperty, StyledProperty<double> minHeightProperty,
                                           StyledProperty<double> maxHeightProperty, StyledProperty<bool> topmostProperty)
        {
            // Topmost property is not supported
            var bindings = new List<IDisposable>();

            void Bind(AvaloniaProperty what, AvaloniaProperty to) => bindings.Add(this.Bind(what, popup[~to]));
            Bind(WidthProperty, widthProperty);
            Bind(MinWidthProperty, minWidthProperty);
            Bind(MaxWidthProperty, maxWidthProperty);
            Bind(HeightProperty, heightProperty);
            Bind(MinHeightProperty, minHeightProperty);
            Bind(MaxHeightProperty, maxHeightProperty);
            
            return Disposable.Create(() =>
            {
                foreach (var x in bindings)
                    x.Dispose();
            });
        }

        public void ConfigurePosition(IVisual target, PlacementMode placement, Point offset,
                                      PopupAnchor anchor = PopupAnchor.None, PopupGravity gravity = PopupGravity.None,
                                      PopupPositionerConstraintAdjustment constraintAdjustment = PopupPositionerConstraintAdjustment.All,
                                      Rect? rect = null)
        {
            _positionerParameters.ConfigurePosition((TopLevel)_overlayLayer.GetVisualRoot(), target, placement, offset, anchor,
                gravity, constraintAdjustment, rect);
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
                _popupPositioner.Update(_positionerParameters);
            }
        }
    }
}