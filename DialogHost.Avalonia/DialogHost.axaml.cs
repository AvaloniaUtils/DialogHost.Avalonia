using System;
using System.Collections.Generic;
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
using Avalonia.Threading;
using DialogHostAvalonia.DialogEvents;
using DialogHostAvalonia.Positioners;
using DialogHostAvalonia.Utilities;

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

        /// <summary>
        /// Defines the <see cref="Identifier"/> property
        /// </summary>
        public static readonly DirectProperty<DialogHost, string?> IdentifierProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, string?>(
                nameof(Identifier),
                o => o.Identifier,
                (o, v) => o.Identifier = v);

        /// <summary>
        /// Gets or sets the <see cref="DialogHost"/> unique identifier
        /// </summary>
        public string? Identifier {
            get => _identifier;
            set => SetAndRaise(IdentifierProperty, ref _identifier, value);
        }

        /// <summary>
        /// Defines the <see cref="ClosingAnimation"/> property
        /// </summary>
        public static readonly StyledProperty<IAnimation?> ClosingAnimationProperty =
            AvaloniaProperty.Register<DialogHost, IAnimation?>(nameof(ClosingAnimation));

        /// <summary>
        /// Gets or sets popup closing animation
        /// </summary>
        public IAnimation? ClosingAnimation {
            get => GetValue(ClosingAnimationProperty);
            set => SetValue(ClosingAnimationProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="OpeningAnimation"/> property
        /// </summary>
        public static readonly StyledProperty<IAnimation?> OpeningAnimationProperty =
            AvaloniaProperty.Register<DialogHost, IAnimation?>(nameof(OpeningAnimation));

        /// <summary>
        /// Gets or sets popup opening animation
        /// </summary>
        public IAnimation? OpeningAnimation {
            get => GetValue(OpeningAnimationProperty);
            set => SetValue(OpeningAnimationProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="DialogContent"/> property
        /// </summary>
        public static readonly StyledProperty<object?> DialogContentProperty =
            AvaloniaProperty.Register<DialogHost, object?>(nameof(DialogContent));

        /// <summary>
        /// Gets or sets content to display in popup
        /// </summary>
        public object? DialogContent {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="DialogContentTemplate"/> property
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> DialogContentTemplateProperty =
            AvaloniaProperty.Register<DialogHost, IDataTemplate?>(nameof(DialogContentTemplate));

        /// <summary>
        /// Gets or sets the data template used to display the content of the control.
        /// </summary>
        public IDataTemplate? DialogContentTemplate {
            get => GetValue(DialogContentTemplateProperty);
            set => SetValue(DialogContentTemplateProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="OverlayBackground"/> property
        /// </summary>
        public static readonly StyledProperty<IBrush> OverlayBackgroundProperty =
            AvaloniaProperty.Register<DialogHost, IBrush>(nameof(OverlayBackground));

        /// <summary>
        /// Gets or sets <see cref="IBrush"/> for <see cref="ContentCoverName"/>
        /// </summary>
        public IBrush OverlayBackground {
            get => GetValue(OverlayBackgroundProperty);
            set => SetValue(OverlayBackgroundProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="DialogMargin"/> property
        /// </summary>
        public static readonly StyledProperty<Thickness> DialogMarginProperty =
            AvaloniaProperty.Register<DialogHost, Thickness>(nameof(DialogMargin));

        /// <summary>
        /// Gets or sets popup margins
        /// </summary>
        public Thickness DialogMargin {
            get => GetValue(DialogMarginProperty);
            set => SetValue(DialogMarginProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="IsOpen"/> property
        /// </summary>
        public static readonly StyledProperty<bool> IsOpenProperty =
            AvaloniaProperty.Register<DialogHost, bool>(nameof(IsOpen));

        /// <summary>
        /// Get or set is dialog currently open or not
        /// </summary>
        public bool IsOpen {
            get => GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        #region DialogOpened* Properties

        /// <summary>
        /// Defines the <see cref="DialogOpened"/> event
        /// </summary>
        public static readonly RoutedEvent DialogOpenedEvent =
            RoutedEvent.Register<DialogHost, DialogOpenedEventArgs>(nameof(DialogOpened), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when a dialog is opened.
        /// </summary>
        public event DialogOpenedEventHandler DialogOpened {
            add => AddHandler(DialogOpenedEvent, value);
            remove => RemoveHandler(DialogOpenedEvent, value);
        }

        /// <summary>
        /// Called when dialog is opened
        /// </summary>
        /// <param name="dialogOpenedEventArgs">Event arguments</param>
        protected void OnDialogOpened(DialogOpenedEventArgs dialogOpenedEventArgs) => RaiseEvent(dialogOpenedEventArgs);

        /// <summary>
        /// Defines the <see cref="DialogOpenedCallback"/> property
        /// </summary>
        public static readonly StyledProperty<DialogOpenedEventHandler?> DialogOpenedCallbackProperty =
            AvaloniaProperty.Register<DialogHost, DialogOpenedEventHandler?>(nameof(DialogOpenedCallback));

        /// <summary>
        /// Gets or set dialog opening callback
        /// </summary>
        public DialogOpenedEventHandler? DialogOpenedCallback {
            get => GetValue(DialogOpenedCallbackProperty);
            set => SetValue(DialogOpenedCallbackProperty, value);
        }

        #endregion

        #region DialogClosing* Properties

        /// <summary>
        /// Defines the <see cref="DialogClosing"/> event
        /// </summary>
        public static readonly RoutedEvent DialogClosingEvent =
            RoutedEvent.Register<DialogHost, DialogClosingEventArgs>(nameof(DialogClosing), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised just before a dialog is closed.
        /// </summary>
        public event EventHandler<DialogClosingEventArgs> DialogClosing {
            add => AddHandler(DialogClosingEvent, value);
            remove => RemoveHandler(DialogClosingEvent, value);
        }

        /// <summary>
        /// Called when dialog is closing
        /// </summary>
        /// <param name="eventArgs">Event arguments</param>
        protected void OnDialogClosing(DialogClosingEventArgs eventArgs) => RaiseEvent(eventArgs);

        /// <summary>
        /// Defines the <see cref="DialogClosingCallback"/> property
        /// </summary>
        public static readonly StyledProperty<DialogClosingEventHandler?> DialogClosingCallbackProperty =
            AvaloniaProperty.Register<DialogHost, DialogClosingEventHandler?>(nameof(DialogClosingCallback));

        /// <summary>
        /// Gets or sets callback which will be invoked when dialog attempting to close
        /// </summary>
        public DialogClosingEventHandler? DialogClosingCallback {
            get => GetValue(DialogClosingCallbackProperty);
            set => SetValue(DialogClosingCallbackProperty, value);
        }

        #endregion

        #region DialogClosed* Properties

        /// <summary>
        /// Defines the <see cref="DialogClosed"/> event
        /// </summary>
        public static readonly RoutedEvent DialogClosedEvent =
            RoutedEvent.Register<DialogHost, DialogClosingEventArgs>(nameof(DialogClosed), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised just after a dialog is closed.
        /// </summary>
        public event EventHandler<DialogClosedEventArgs> DialogClosed {
            add => AddHandler(DialogClosedEvent, value);
            remove => RemoveHandler(DialogClosedEvent, value);
        }

        /// <summary>
        /// Called when dialog is closed
        /// </summary>
        /// <param name="dialogOpenedEventArgs">Event arguments</param>
        protected void OnDialogClosed(DialogClosedEventArgs dialogOpenedEventArgs) => RaiseEvent(dialogOpenedEventArgs);

        /// <summary>
        /// Defines the <see cref="DialogClosedCallback"/> property
        /// </summary>
        public static readonly StyledProperty<DialogClosedEventHandler?> DialogClosedCallbackProperty
            = AvaloniaProperty.Register<DialogHost, DialogClosedEventHandler?>(nameof(DialogClosedCallback));

        /// <summary>
        /// Gets or sets callback which will be invoked when dialog closed
        /// </summary>
        public DialogClosedEventHandler? DialogClosedCallback {
            get => GetValue(DialogClosedCallbackProperty);
            set => SetValue(DialogClosedCallbackProperty, value);
        }

        #endregion

        /// <summary>
        /// Defines the <see cref="CloseOnClickAway"/> property
        /// </summary>
        public static readonly StyledProperty<bool> CloseOnClickAwayProperty =
            AvaloniaProperty.Register<DialogHost, bool>(nameof(CloseOnClickAway));

        /// <summary>
        /// Gets or sets value, indicating is dialog can be dismissed by clicking on the content cover outside the popup
        /// </summary>
        public bool CloseOnClickAway {
            get => GetValue(CloseOnClickAwayProperty);
            set => SetValue(CloseOnClickAwayProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="CloseOnClickAwayParameter"/> property
        /// </summary>
        public static readonly StyledProperty<object?> CloseOnClickAwayParameterProperty =
            AvaloniaProperty.Register<DialogHost, object?>(nameof(CloseOnClickAwayParameter));

        /// <summary>
        /// Gets or sets value, what will be used as dialog result if user close dialog via clicking on content cover outside the popup
        /// </summary>
        /// <remarks>
        /// Popup can be closed this way only if <see cref="CloseOnClickAway"/> is set to <c>true</c>
        /// </remarks>
        public object? CloseOnClickAwayParameter {
            get => GetValue(CloseOnClickAwayParameterProperty);
            set => SetValue(CloseOnClickAwayParameterProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="OpenDialogCommand"/> property
        /// </summary>
        public static readonly DirectProperty<DialogHost, ICommand> OpenDialogCommandProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
                nameof(OpenDialogCommand),
                o => o.OpenDialogCommand);

        /// <summary>
        /// Gets command, what can be executed to open dialog
        /// </summary>
        public ICommand OpenDialogCommand {
            get => _openDialogCommand;
            private set => SetAndRaise(OpenDialogCommandProperty, ref _openDialogCommand, value);
        }

        /// <summary>
        /// Defines the <see cref="CloseDialogCommand"/> property
        /// </summary>
        public static readonly DirectProperty<DialogHost, ICommand> CloseDialogCommandProperty =
            AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
                nameof(CloseDialogCommand),
                o => o.CloseDialogCommand);

        /// <summary>
        /// Gets command, what can be executed to close dialog
        /// </summary>
        public ICommand CloseDialogCommand {
            get => _closeDialogCommand;
            private set => SetAndRaise(CloseDialogCommandProperty, ref _closeDialogCommand, value);
        }

        /// <summary>
        /// Defines the <see cref="PopupTemplate"/> property
        /// </summary>
        public static readonly StyledProperty<IControlTemplate?> PopupTemplateProperty =
            AvaloniaProperty.Register<DialogHost, IControlTemplate?>(nameof(PopupTemplate));

        /// <summary>
        /// Gets or sets the template used for popup
        /// </summary>
        public IControlTemplate? PopupTemplate {
            get => GetValue(PopupTemplateProperty);
            set => SetValue(PopupTemplateProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="PopupPositioner"/> property
        /// </summary>
        public static readonly StyledProperty<IDialogPopupPositioner?> PopupPositionerProperty =
            AvaloniaProperty.Register<DialogHost, IDialogPopupPositioner?>(nameof(PopupPositioner));

        /// <summary>
        /// Allows to override popup positioner
        /// </summary>
        public IDialogPopupPositioner? PopupPositioner {
            get => GetValue(PopupPositionerProperty);
            set => SetValue(PopupPositionerProperty, value);
        }

        #region Static methods: Show, Close, etc

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="dialogIdentifier"><see cref="Identifier"/> of the instance where the dialog should be shown. Typically this will match an identifier set in XAML. <c>null</c> is allowed.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closedEventHandler">Allows access to closed event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, string? dialogIdentifier = null,
                                         DialogOpenedEventHandler? openedEventHandler = null,
                                         DialogClosingEventHandler? closingEventHandler = null,
                                         DialogClosedEventHandler? closedEventHandler = null) {
            if (content is null) throw new ArgumentNullException(nameof(content));
            return GetInstance(dialogIdentifier).Show(content, openedEventHandler, closingEventHandler, closedEventHandler);
        }

        /// <summary>
        /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="instance">Instance of <see cref="DialogHost"/> where the dialog should be shown.</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closedEventHandler">Allows access to closed event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public static Task<object?> Show(object content, DialogHost instance,
                                         DialogOpenedEventHandler? openedEventHandler = null,
                                         DialogClosingEventHandler? closingEventHandler = null,
                                         DialogClosedEventHandler? closedEventHandler = null) {
            if (content is null) throw new ArgumentNullException(nameof(content));
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            return instance.Show(content, openedEventHandler, closingEventHandler, closedEventHandler);
        }

        /// <summary>Close a modal dialog.</summary>
        /// <param name="dialogIdentifier">of the instance where the dialog should be closed. Typically this will match an identifier set in XAML.</param>
        public static void Close(string? dialogIdentifier)
            => Close(dialogIdentifier, null);

        /// <summary>
        /// Close a modal dialog.
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

        private static readonly HashSet<DialogHost> LoadedInstances = new();
        private static DialogHost GetInstance(string? dialogIdentifier) {
            if (LoadedInstances.Count == 0)
                throw new InvalidOperationException("No loaded DialogHost instances.");

            var targets = LoadedInstances.Where(dh => dialogIdentifier == null || Equals(dh.Identifier, dialogIdentifier)).ToList();
            if (targets.Count == 0)
                throw new InvalidOperationException($"No loaded DialogHost have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
            if (targets.Count > 1)
                throw new InvalidOperationException("Multiple viable DialogHosts. Specify a unique Identifier on each DialogHost, especially where multiple Windows are a concern.");

            return targets[0];
        }

        #endregion

        private DialogClosedEventHandler? _asyncShowClosedEventHandler;
        private DialogClosingEventHandler? _asyncShowClosingEventHandler;
        private DialogOpenedEventHandler? _asyncShowOpenedEventHandler;

        private ICommand _closeDialogCommand;
        private ICommand _openDialogCommand;

        private IDisposable? _openingAnimationDisposable;
        private IDisposable? _closingAnimationDisposable;
        private TaskCompletionSource<object?>? _dialogTaskCompletionSource;

        private string? _identifier;
        private IDisposable? _templateDisposables;
        private ContentControl _overlayPopupHost = null!;
        private IInputElement? _restoreFocusDialogClose;
        private Panel _rootContainer = null!;

        static DialogHost() {
            IsOpenProperty.Changed.AddClassHandler<DialogHost>(IsOpenPropertyChangedCallback);
        }

        /// <inheritdoc />
        public DialogHost() {
            _closeDialogCommand = new DialogHostCommandImpl(InternalClose, _ => IsOpen, this.GetObservable(IsOpenProperty));
            _openDialogCommand = new DialogHostCommandImpl(o => _ = Show(o, null, null, null), _ => !IsOpen, this.GetObservable(IsOpenProperty));
        }

        /// <summary>
        /// Gets a DialogSession for the currently open dialog for managing it programmatically. If no dialog is open, CurrentSession will return null
        /// </summary>
        public DialogSession? CurrentSession { get; private set; }

        /// <summary>
        /// Shows a modal dialog in the this <see cref="DialogHost"/>
        /// </summary>
        /// <param name="content">Content to show (can be a control or view model).</param>
        /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
        /// <param name="closedEventHandler">Allows access to closed event which would otherwise have been subscribed to on a instance.</param>
        /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
        public async Task<object?> Show(object content,
                                        DialogOpenedEventHandler? openedEventHandler,
                                        DialogClosingEventHandler? closingEventHandler,
                                        DialogClosedEventHandler? closedEventHandler) {
            if (IsOpen)
                throw new InvalidOperationException("DialogHost is already open.");

            _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

            DialogContent = content ?? throw new ArgumentNullException(nameof(content));

            _asyncShowOpenedEventHandler = openedEventHandler;
            _asyncShowClosingEventHandler = closingEventHandler;
            _asyncShowClosedEventHandler = closedEventHandler;
            IsOpen = true;

            object? result = await _dialogTaskCompletionSource.Task;

            _asyncShowOpenedEventHandler = null;
            _asyncShowClosingEventHandler = null;
            _asyncShowClosedEventHandler = null;

            return result;
        }

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
            }
            else {
                object? closeParameter = null;
                if (dialogHost.CurrentSession is { } session) {
                    //multiple ways of calling back that the dialog is closed:
                    // * routed event
                    // * the attached property (which should be applied to the button which opened the dialog
                    // * straight forward IsOpen dependency property 
                    // * handler provided to the async show method
                    var dialogClosedEventArgs = new DialogClosedEventArgs(dialogHost.CurrentSession, DialogClosedEvent);
                    dialogHost.OnDialogClosed(dialogClosedEventArgs);
                    dialogHost.DialogClosedCallback?.Invoke(dialogHost, dialogClosedEventArgs);
                    dialogHost._asyncShowClosedEventHandler?.Invoke(dialogHost, dialogClosedEventArgs);

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
            InvalidateVisual();
        }

        /// <summary>
        /// Notify <see cref="OpenDialogCommand"/> and <see cref="CloseDialogCommand"/> about "Can execute" status change
        /// </summary>
        protected void RaiseCommandsCanExecuteChanged() {
            (_openDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
            (_closeDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            LoadedInstances.Add(this);
        }

        /// <inheritdoc />
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnDetachedFromVisualTree(e);
            LoadedInstances.Remove(this);
        }
    }
}