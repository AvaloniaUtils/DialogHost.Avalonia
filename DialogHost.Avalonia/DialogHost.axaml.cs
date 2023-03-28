using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using DialogHostAvalonia.Positioners;

namespace DialogHostAvalonia {
    /// <summary>
    /// Main control to display dialogs
    /// </summary>
    /// <example>
    /// DialogHost should wrap around content. How example DialogHost for whole window:
    /// <code>
    /// &lt;Window ...&gt;
    ///   &lt;dialogHostAvalonia:DialogHost Identifier="MainDialogHost"&gt;
    ///     &lt;!-- Actual window's content --&gt;
    ///     &lt;!-- For example --&gt;
    ///     &lt;Grid&gt;
    ///       &lt;!-- Other content --&gt;
    ///     &lt;/Grid&gt;
    ///   &lt;/dialogHostAvalonia:DialogHost&gt;
    /// &lt;/Window&gt;
    /// </code>
    /// </example>
    [TemplatePart(ContentCoverName, typeof(Grid))]
    [TemplatePart(DialogOverlayPopupHostName, typeof(ContentControl))]
    [TemplatePart(RootContainerName, typeof(Panel))]
    public class DialogHost : ContentControl {
        /// <summary>
        /// Element name for content cover element
        /// </summary>
        public const string ContentCoverName = "PART_ContentCover";

        /// <summary>
        /// Element name for DialogOverlayPopupHost
        /// </summary>
        public const string DialogOverlayPopupHostName = "PART_DialogOverlayPopupHost";

        /// <summary>
        /// Element name for root container, which suited for adding or removing <see cref="DialogOverlayPopupHostName"/>
        /// </summary>
        public const string RootContainerName = "PART_RootContainer";

        private static readonly HashSet<DialogHost> LoadedInstances = new();

        public static readonly DirectProperty<DialogHost, string?> IdentifierProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, string?>(
                nameof(Identifier),
                o => o.Identifier,
                (o, v) => o.Identifier = v);

        public static readonly StyledProperty<IAnimation?> ClosingAnimationProperty = 
            AvaloniaProperty.Register<DialogHost, IAnimation?>(nameof(ClosingAnimation));

        public static readonly StyledProperty<IAnimation?> OpeningAnimationProperty = 
            AvaloniaProperty.Register<DialogHost, IAnimation?>(nameof(OpeningAnimation));

        public static readonly StyledProperty<object?> DialogContentProperty =
            AvaloniaProperty.Register<DialogHost, object?>(nameof(DialogContent));

        public static readonly StyledProperty<IDataTemplate?> DialogContentTemplateProperty =
            AvaloniaProperty.Register<DialogHost, IDataTemplate?>(nameof(DialogContentTemplate));

        public static readonly StyledProperty<IBrush> OverlayBackgroundProperty =
            AvaloniaProperty.Register<DialogHost, IBrush>(nameof(OverlayBackground));

        public static readonly StyledProperty<Thickness> DialogMarginProperty =
            AvaloniaProperty.Register<DialogHost, Thickness>(nameof(DialogMargin));

        /// <summary>
        /// Defines the <see cref="IsOpen"/> property
        /// </summary>
        public static readonly StyledProperty<bool> IsOpenProperty =
            AvaloniaProperty.Register<DialogHost, bool>(nameof(IsOpen));

        public static readonly RoutedEvent DialogOpenedEvent =
            RoutedEvent.Register<DialogHost, DialogOpenedEventArgs>(nameof(DialogOpened), RoutingStrategies.Bubble);

        public static readonly DirectProperty<DialogHost, bool> CloseOnClickAwayProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, bool>(
                nameof(CloseOnClickAway),
                o => o.CloseOnClickAway,
                (o, v) => o.CloseOnClickAway = v);

        public static readonly DirectProperty<DialogHost, object?> CloseOnClickAwayParameterProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, object?>(
                nameof(CloseOnClickAwayParameter),
                o => o.CloseOnClickAwayParameter,
                (o, v) => o.CloseOnClickAwayParameter = v);

        public static readonly RoutedEvent DialogClosingEvent =
            RoutedEvent.Register<DialogHost, DialogClosingEventArgs>(nameof(DialogClosing), RoutingStrategies.Bubble);

        public static readonly DirectProperty<DialogHost, DialogClosingEventHandler> DialogClosingCallbackProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, DialogClosingEventHandler>(
                nameof(DialogClosingCallback),
                o => o.DialogClosingCallback,
                (o, v) => o.DialogClosingCallback = v);

        public static readonly DirectProperty<DialogHost, DialogOpenedEventHandler?> DialogOpenedCallbackProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, DialogOpenedEventHandler?>(
                nameof(DialogOpenedCallback),
                o => o.DialogOpenedCallback,
                (o, v) => o.DialogOpenedCallback = v);

        public static readonly DirectProperty<DialogHost, ICommand> OpenDialogCommandProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
                nameof(OpenDialogCommand),
                o => o.OpenDialogCommand);

        public static readonly DirectProperty<DialogHost, ICommand> CloseDialogCommandProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
                nameof(CloseDialogCommand),
                o => o.CloseDialogCommand);

        public static readonly StyledProperty<IControlTemplate?> PopupTemplateProperty =
            AvaloniaProperty.Register<DialogHost, IControlTemplate?>(nameof(PopupTemplate));

        public static readonly DirectProperty<DialogHost, IDialogPopupPositioner?> PopupPositionerProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, IDialogPopupPositioner?>(
                nameof(PopupPositioner),
                o => o.PopupPositioner,
                (o, v) => o.PopupPositioner = v);

        private DialogClosingEventHandler? _asyncShowClosingEventHandler;
        private DialogOpenedEventHandler? _asyncShowOpenedEventHandler;

        private ICommand _closeDialogCommand;

        private bool _closeOnClickAway;

        private object? _closeOnClickAwayParameter;
        private IDisposable? _closingAnimationDisposable;

        private DialogClosingEventHandler? _dialogClosingCallback;

        private DialogOpenedEventHandler? _dialogOpenedCallback;

        private TaskCompletionSource<object?>? _dialogTaskCompletionSource;

        private string? _identifier;

        private ICommand _openDialogCommand;

        private IDisposable? _openingAnimationDisposable;

        private ContentControl _overlayPopupHost;

        private IDialogPopupPositioner? _popupPositioner;
        private IInputElement? _restoreFocusDialogClose;
        private Panel _rootContainer;

        private IDisposable? _templateDisposables;

        static DialogHost() {
            IsOpenProperty.Changed.AddClassHandler<DialogHost>(IsOpenPropertyChangedCallback);
        }

        public DialogHost() {
            _closeDialogCommand = new DialogHostCommandImpl(InternalClose, o => IsOpen, this.GetObservable(IsOpenProperty));
            _openDialogCommand = new DialogHostCommandImpl(o => ShowInternal(o, null, null), o => !IsOpen, this.GetObservable(IsOpenProperty));
        }

        public IAnimation? ClosingAnimation {
            get => GetValue(ClosingAnimationProperty);
            set => SetValue(ClosingAnimationProperty, value);
        }

        public IAnimation? OpeningAnimation {
            get => GetValue(OpeningAnimationProperty);
            set => SetValue(OpeningAnimationProperty, value);
        }

        public IControlTemplate? PopupTemplate {
            get => GetValue(PopupTemplateProperty);
            set => SetValue(PopupTemplateProperty, value);
        }

        public DialogOpenedEventHandler? DialogOpenedCallback {
            get => _dialogOpenedCallback;
            set => SetAndRaise(DialogOpenedCallbackProperty, ref _dialogOpenedCallback, value);
        }

        public ICommand OpenDialogCommand {
            get => _openDialogCommand;
            private set => SetAndRaise<ICommand>(OpenDialogCommandProperty, ref _openDialogCommand, value);
        }

        public ICommand CloseDialogCommand {
            get => _closeDialogCommand;
            private set => SetAndRaise<ICommand>(CloseDialogCommandProperty, ref _closeDialogCommand, value);
        }

        public string? Identifier {
            get => _identifier;
            set => SetAndRaise(IdentifierProperty, ref _identifier, value);
        }

        public object? DialogContent {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        public IDataTemplate? DialogContentTemplate {
            get => GetValue(DialogContentTemplateProperty);
            set => SetValue(DialogContentTemplateProperty, value);
        }

        public IBrush OverlayBackground {
            get => GetValue(OverlayBackgroundProperty);
            set => SetValue(OverlayBackgroundProperty, value);
        }

        public Thickness DialogMargin {
            get => GetValue(DialogMarginProperty);
            set => SetValue(DialogMarginProperty, value);
        }

        /// <summary>
        /// Get or set is dialog currently open or not
        /// </summary>
        public bool IsOpen {
            get => GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public bool CloseOnClickAway {
            get => _closeOnClickAway;
            set => SetAndRaise(CloseOnClickAwayProperty, ref _closeOnClickAway, value);
        }

        public object? CloseOnClickAwayParameter {
            get => _closeOnClickAwayParameter;
            set => SetAndRaise(CloseOnClickAwayParameterProperty, ref _closeOnClickAwayParameter, value);
        }

        /// <summary>
        /// Allows to override popup positioner
        /// </summary>
        public IDialogPopupPositioner? PopupPositioner {
            get => _popupPositioner;
            set => SetAndRaise(PopupPositionerProperty, ref _popupPositioner, value);
        }

        /// <summary>
        /// Returns a DialogSession for the currently open dialog for managing it programmatically. If no dialog is open, CurrentSession will return null
        /// </summary>
        public DialogSession? CurrentSession { get; private set; }

        public DialogClosingEventHandler DialogClosingCallback {
            get => _dialogClosingCallback;
            set => SetAndRaise(DialogClosingCallbackProperty, ref _dialogClosingCallback, value);
        }

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content)
            => Show(content, dialogIdentifier: null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>        
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>        
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogOpenedEventHandler openedEventHandler)
            => Show(content, (string?) null, openedEventHandler, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogClosingEventHandler closingEventHandler)
            => Show(content, (string?) null, null, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>        
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogOpenedEventHandler? openedEventHandler, DialogClosingEventHandler? closingEventHandler)
            => Show(content, (string?) null, openedEventHandler, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifier set in XAML. <c>null</c> is allowed.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier)
            => Show(content, dialogIdentifier, null, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifier set in XAML. <c>null</c> is allowed.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier, DialogOpenedEventHandler openedEventHandler)
            => Show(content, dialogIdentifier, openedEventHandler, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifier set in XAML. <c>null</c> is allowed.</param>        
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier, DialogClosingEventHandler closingEventHandler)
            => Show(content, dialogIdentifier, null, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifier set in XAML. <c>null</c> is allowed.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier, DialogOpenedEventHandler? openedEventHandler,
                                               DialogClosingEventHandler? closingEventHandler) {
            if (content is null) throw new ArgumentNullException(nameof(content));
            return GetInstance(dialogIdentifier).ShowInternal(content, openedEventHandler, closingEventHandler);
        }

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="instance">Instance of <see cref="DialogHost"/> where the dialog should be shown.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogHost instance)
            => Show(content, instance, null, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="instance">Instance of <see cref="DialogHost"/> where the dialog should be shown.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogHost instance, DialogOpenedEventHandler openedEventHandler)
            => Show(content, instance, openedEventHandler, null);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="instance">Instance of <see cref="DialogHost"/> where the dialog should be shown.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogHost instance, DialogClosingEventHandler closingEventHandler)
            => Show(content, instance, null, closingEventHandler);

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="instance">Instance of <see cref="DialogHost"/> where the dialog should be shown.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogHost instance, DialogOpenedEventHandler? openedEventHandler,
                                               DialogClosingEventHandler? closingEventHandler) {
            if (content is null) throw new ArgumentNullException(nameof(content));
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            return instance.ShowInternal(content, openedEventHandler, closingEventHandler);
        }

        /// <summary>Close a modal dialog.</summary>
        /// <param name="dialogIdentifier">of the instance where the dialog should be closed. Typically this will match an identifier set in XAML.</param>
        public static void Close(string? dialogIdentifier)
            => Close(dialogIdentifier, null);

        /// <summary>
        ///  Close a modal dialog.
        /// </summary>
        /// <param name="dialogIdentifier"> of the instance where the dialog should be closed. Typically this will match an identifier set in XAML. </param>
        /// <param name="parameter"> to provide to close handler</param>
        public static void Close(string? dialogIdentifier, object? parameter) {
            DialogHost dialogHost = GetInstance(dialogIdentifier);
            if (dialogHost.CurrentSession is { } currentSession) {
                currentSession.Close(parameter);
                return;
            }

            throw new InvalidOperationException("DialogHost is not open.");
        }

        /// <summary>
        /// Retrieve the current dialog session for a DialogHost
        /// </summary>
        /// <param name="dialogIdentifier">The identifier to use to retrieve the DialogHost</param>
        /// <returns>The DialogSession if one is in process, or null</returns>
        public static DialogSession? GetDialogSession(string? dialogIdentifier) {
            DialogHost dialogHost = GetInstance(dialogIdentifier);
            return dialogHost.CurrentSession;
        }

        /// <summary>
        /// dialog instance exists
        /// </summary>
        /// <param name="dialogIdentifier">of the instance where the dialog should be closed. Typically this will match an identifier set in XAML.</param>
        /// <returns></returns>
        public static bool IsDialogOpen(string? dialogIdentifier) => GetDialogSession(dialogIdentifier)?.IsEnded == false;

        private static DialogHost GetInstance(string? dialogIdentifier) {
            if (LoadedInstances.Count == 0)
                throw new InvalidOperationException("No loaded DialogHost instances.");

            var targets = LoadedInstances.Where(dh => dialogIdentifier == null || Equals(dh.Identifier, dialogIdentifier)).ToList();
            if (targets.Count == 0)
                throw new InvalidOperationException(
                    $"No loaded DialogHost have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
            if (targets.Count > 1)
                throw new InvalidOperationException(
                    "Multiple viable DialogHosts. Specify a unique Identifier on each DialogHost, especially where multiple Windows are a concern.");

            return targets[0];
        }

        internal async Task<object?> ShowInternal(object content, DialogOpenedEventHandler? openedEventHandler,
                                                  DialogClosingEventHandler? closingEventHandler) {
            if (IsOpen)
                throw new InvalidOperationException("DialogHost is already open.");

            _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

            if (content != null)
                DialogContent = content;

            _asyncShowOpenedEventHandler = openedEventHandler;
            _asyncShowClosingEventHandler = closingEventHandler;
            IsOpen = true;

            object? result = await _dialogTaskCompletionSource.Task;

            _asyncShowOpenedEventHandler = null;
            _asyncShowClosingEventHandler = null;

            return result;
        }

        private static void IsOpenPropertyChangedCallback(DialogHost dialogHost, AvaloniaPropertyChangedEventArgs args) {
            if (args.GetNewValue<bool>()) {
                dialogHost.CurrentSession = new DialogSession(dialogHost);
                dialogHost._restoreFocusDialogClose = FocusManager.Instance?.Current;

                dialogHost.AttachPopupHost();

                //multiple ways of calling back that the dialog has opened:
                // * routed event
                // * straight forward dependency property 
                // * handler provided to the async show method
                var dialogOpenedEventArgs = new DialogOpenedEventArgs(dialogHost.CurrentSession, DialogOpenedEvent);
                dialogHost.OnDialogOpened(dialogOpenedEventArgs);
                dialogHost.DialogOpenedCallback?.Invoke(dialogHost, dialogOpenedEventArgs);
                dialogHost._asyncShowOpenedEventHandler?.Invoke(dialogHost, dialogOpenedEventArgs);

                // dialogHost._overlayPopupHost?.ConfigurePosition(dialogHost._overlayLayer, PlacementMode.AnchorAndGravity, new Point());
            }
            else {
                object? closeParameter = null;
                if (dialogHost.CurrentSession is { } session) {
                    if (!session.IsEnded) {
                        session.Close(session.CloseParameter);
                    }

                    //DialogSession.Close may attempt to cancel the closing of the dialog.
                    //When the dialog is closed in this manner it is not valid
                    if (!session.IsEnded) {
                        throw new InvalidOperationException($"Cannot cancel dialog closing after {nameof(IsOpen)} property has been set to {bool.FalseString}");
                    }

                    closeParameter = session.CloseParameter;
                    dialogHost.CurrentSession = null;
                }

                dialogHost.DetachPopupHost();

                //NB: _dialogTaskCompletionSource is only set in the case where the dialog is shown with Show
                dialogHost._dialogTaskCompletionSource?.TrySetResult(closeParameter);

                dialogHost._restoreFocusDialogClose?.Focus();
            }

            dialogHost.RaiseCommandsCanExecuteChanged();
        }
        private void AttachPopupHost() {
            if (_closingAnimationDisposable != null) {
                // If closing animation running - stop it and call opening animation after
                _closingAnimationDisposable.Dispose();
                _closingAnimationDisposable = null;
                _openingAnimationDisposable = OpeningAnimation?.Apply(_overlayPopupHost, null, Observable.Return(true), () => {
                    _openingAnimationDisposable = null;
                });
            }
            else if (_overlayPopupHost.Parent == null) {
                // If not shown - show and call opening animation after
                Dispatcher.UIThread.Post(_ => {
                    _rootContainer.Children.Add(_overlayPopupHost);
                    _openingAnimationDisposable = OpeningAnimation?.Apply(_overlayPopupHost, null, Observable.Return(true), () => {
                        _openingAnimationDisposable = null;
                    });
                }, DispatcherPriority.Layout);
            }
            else {
                // If shown already and no closing in progress - no nothing
                return;
            }
        }

        private void DetachPopupHost() {
            // Cancel opening animation if any
            _openingAnimationDisposable?.Dispose();
            _openingAnimationDisposable = null;

            if (_overlayPopupHost.Parent != null) {
                if (ClosingAnimation != null) {
                    // Starting closing animation if set
                    _closingAnimationDisposable = ClosingAnimation.Apply(_overlayPopupHost, null, Observable.Return(true), () => {
                        Dispatcher.UIThread.Post(_ => _rootContainer.Children.Remove(_overlayPopupHost), DispatcherPriority.Layout);
                        _closingAnimationDisposable = null;
                    });
                }
                else {
                    // Or just remove popup host
                    Dispatcher.UIThread.Post(_ => _rootContainer.Children.Remove(_overlayPopupHost), DispatcherPriority.Layout);
                }
            }
            this.InvalidateVisual();
        }

        protected void RaiseCommandsCanExecuteChanged() {
            (_openDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
            (_closeDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            _templateDisposables?.Dispose();

            _rootContainer = e.NameScope.Find<Panel>(RootContainerName)
                          ?? throw new InvalidOperationException($"{RootContainerName} not found in {nameof(DialogHost)} template");
            _overlayPopupHost = e.NameScope.Find<ContentControl>(DialogOverlayPopupHostName) 
                             ?? throw new InvalidOperationException($"{DialogOverlayPopupHostName} not found in {nameof(DialogHost)} template");
            // Removing the overlay layer initially
            _rootContainer.Children.Remove(_overlayPopupHost);
            
            _templateDisposables = e.NameScope.Find<InputElement>(ContentCoverName)?.AddDisposableHandler(PointerReleasedEvent, ContentCoverGrid_OnPointerReleased);

            if (IsOpen) {
                AttachPopupHost();
            }
            
            base.OnApplyTemplate(e);
        }

        private void ContentCoverGrid_OnPointerReleased(object sender, PointerReleasedEventArgs e) {
            if (CloseOnClickAway && CurrentSession != null) {
                InternalClose(CloseOnClickAwayParameter);
            }
        }

        protected void OnDialogOpened(DialogOpenedEventArgs dialogOpenedEventArgs) => RaiseEvent(dialogOpenedEventArgs);

        /// <summary>
        /// Raised when a dialog is opened.
        /// </summary>
        public event DialogOpenedEventHandler DialogOpened {
            add => AddHandler(DialogOpenedEvent, value);
            remove => RemoveHandler(DialogOpenedEvent, value);
        }

        /// <summary>
        /// Raised just before a dialog is closed.
        /// </summary>
        public event EventHandler<DialogClosingEventArgs> DialogClosing {
            add => AddHandler(DialogClosingEvent, value);
            remove => RemoveHandler(DialogClosingEvent, value);
        }

        protected void OnDialogClosing(DialogClosingEventArgs eventArgs) => RaiseEvent(eventArgs);

        internal void InternalClose(object? parameter) {
            var currentSession = CurrentSession ?? throw new InvalidOperationException($"{nameof(DialogHost)} does not have a current session");

            currentSession.CloseParameter = parameter;
            currentSession.IsEnded = true;

            //multiple ways of calling back that the dialog is closing:
            // * routed event
            // * straight forward IsOpen dependency property 
            // * handler provided to the async show method
            var dialogClosingEventArgs = new DialogClosingEventArgs(currentSession, DialogClosingEvent);
            OnDialogClosing(dialogClosingEventArgs);
            DialogClosingCallback?.Invoke(this, dialogClosingEventArgs);
            _asyncShowClosingEventHandler?.Invoke(this, dialogClosingEventArgs);

            if (dialogClosingEventArgs.IsCancelled) {
                currentSession.IsEnded = false;
                return;
            }

            IsOpen = false;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            LoadedInstances.Add(this);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            LoadedInstances.Remove(this);
        }
    }
}