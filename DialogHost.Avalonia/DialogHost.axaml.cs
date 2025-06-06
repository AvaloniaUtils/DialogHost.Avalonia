using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using DialogHostAvalonia.Positioners;
using DialogHostAvalonia.Utilities;

namespace DialogHostAvalonia;

/// <summary>
/// A control that hosts dialog content in a modal overlay.
/// </summary>
[TemplatePart(ContentCoverName, typeof(Rectangle))]
[TemplatePart(DialogHostRoot, typeof(Panel))]
public class DialogHost : ContentControl {
    /// <summary>
    /// Gets the name for the content cover part in the control template.
    /// </summary>
    public const string ContentCoverName = "PART_ContentCover";
    
    /// <summary>
    /// Gets the name for the root part in the control template.
    /// </summary>
    public const string DialogHostRoot = "PART_DialogHostRoot";
    
    private const double DefaultBlurRadius = 16.0;
    
    /// <summary>
    /// Tracks all loaded instances of DialogHost.
    /// </summary>
    private static readonly HashSet<DialogHost> _loadedInstances = new();

    /// <summary>
    /// Identifies the <see cref="Identifier"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, string?> IdentifierProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, string?>(
            nameof(Identifier),
            o => o.Identifier,
            (o, v) => o.Identifier = v);

    /// <summary>
    /// Identifies the <see cref="DialogContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DialogContentProperty =
        AvaloniaProperty.Register<DialogHost, object?>(nameof(DialogContent));

    /// <summary>
    /// Identifies the <see cref="DialogContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> DialogContentTemplateProperty =
        AvaloniaProperty.Register<DialogHost, IDataTemplate?>(nameof(DialogContentTemplate));

    /// <summary>
    /// Identifies the <see cref="OverlayBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> OverlayBackgroundProperty =
        AvaloniaProperty.Register<DialogHost, IBrush>(nameof(OverlayBackground));

    /// <summary>
    /// Identifies the <see cref="DialogMargin"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> DialogMarginProperty =
        AvaloniaProperty.Register<DialogHost, Thickness>(nameof(DialogMargin));

    /// <summary>
    /// Identifies the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, bool>(
            nameof(IsOpen),
            o => o.IsOpen,
            (o, v) => o.IsOpen = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="DialogOpened"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<DialogOpenedEventArgs> DialogOpenedEvent =
        RoutedEvent.Register<DialogHost, DialogOpenedEventArgs>(nameof(DialogOpened), RoutingStrategies.Bubble);
    
    /// <summary>
    /// Identifies the <see cref="DialogClosing"/> routed event.
    /// </summary>
    public static readonly RoutedEvent<DialogClosingEventArgs> DialogClosingEvent =
        RoutedEvent.Register<DialogHost, DialogClosingEventArgs>(nameof(DialogClosing), RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="CloseOnClickAway"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, bool> CloseOnClickAwayProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, bool>(
            nameof(CloseOnClickAway),
            o => o.CloseOnClickAway,
            (o, v) => o.CloseOnClickAway = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="CloseOnClickAwayParameter"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, object?> CloseOnClickAwayParameterProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, object?>(
            nameof(CloseOnClickAwayParameter),
            o => o.CloseOnClickAwayParameter,
            (o, v) => o.CloseOnClickAwayParameter = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="DialogClosingCallback"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, DialogClosingEventHandler?> DialogClosingCallbackProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, DialogClosingEventHandler?>(
            nameof(DialogClosingCallback),
            o => o.DialogClosingCallback,
            (o, v) => o.DialogClosingCallback = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="DialogOpenedCallback"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, DialogOpenedEventHandler?> DialogOpenedCallbackProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, DialogOpenedEventHandler?>(
            nameof(DialogOpenedCallback),
            o => o.DialogOpenedCallback,
            (o, v) => o.DialogOpenedCallback = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="OpenDialogCommand"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, ICommand> OpenDialogCommandProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
            nameof(OpenDialogCommand),
            o => o.OpenDialogCommand,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="CloseDialogCommand"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, ICommand> CloseDialogCommandProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, ICommand>(
            nameof(CloseDialogCommand),
            o => o.CloseDialogCommand,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="PopupTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IControlTemplate?> PopupTemplateProperty =
        AvaloniaProperty.Register<DialogHost, IControlTemplate?>(nameof(PopupTemplate));

    /// <summary>
    /// Identifies the <see cref="DisableOpeningAnimation"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, bool> DisableOpeningAnimationProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, bool>(
            nameof(DisableOpeningAnimation),
            o => o.DisableOpeningAnimation,
            (o, v) => o.DisableOpeningAnimation = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="PopupPositioner"/> property.
    /// </summary>
    public static readonly DirectProperty<DialogHost, IDialogPopupPositioner?> PopupPositionerProperty =
        AvaloniaProperty.RegisterDirect<DialogHost, IDialogPopupPositioner?>(
            nameof(PopupPositioner),
            o => o.PopupPositioner,
            (o, v) => o.PopupPositioner = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="BlurBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> BlurBackgroundProperty 
        = AvaloniaProperty.Register<DialogHost, bool>(nameof(BlurBackground));

    /// <summary>
    /// Identifies the <see cref="BlurBackgroundRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BlurBackgroundRadiusProperty 
        = AvaloniaProperty.Register<DialogHost, double>(nameof(BlurBackgroundRadius), DefaultBlurRadius);

    private DialogClosingEventHandler? _asyncShowClosingEventHandler;
    private DialogOpenedEventHandler? _asyncShowOpenedEventHandler;

    private ICommand _closeDialogCommand;

    private bool _closeOnClickAway;

    private object? _closeOnClickAwayParameter;

    private DialogClosingEventHandler? _dialogClosingCallback;

    private DialogOpenedEventHandler? _dialogOpenedCallback;

    private TaskCompletionSource<object?>? _dialogTaskCompletionSource;
    private bool _disableOpeningAnimation;

    private string? _identifier;

    private bool _isOpen;

    private ICommand _openDialogCommand;
    private DialogOverlayPopupHost? _overlayPopupHost;

    private IDialogPopupPositioner? _popupPositioner;
    private IInputElement? _restoreFocusDialogClose;

    private Panel _root;

    private IDisposable? _templateDisposables;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogHost"/> class.
    /// </summary>
    public DialogHost() {
        _closeDialogCommand = new DialogHostCommandImpl(InternalClose, _ => IsOpen, this.GetObservable(IsOpenProperty));
        _openDialogCommand = new DialogHostCommandImpl(o => _ = ShowCore(o, null, null), o => !IsOpen, this.GetObservable(IsOpenProperty));
    }

    /// <summary>
    /// Gets or sets the popup template.
    /// </summary>
    public IControlTemplate? PopupTemplate {
        get => GetValue(PopupTemplateProperty);
        set => SetValue(PopupTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the callback for when the dialog is opened.
    /// </summary>
    public DialogOpenedEventHandler? DialogOpenedCallback {
        get => _dialogOpenedCallback;
        set => SetAndRaise(DialogOpenedCallbackProperty, ref _dialogOpenedCallback, value);
    }

    /// <summary>
    /// Gets the command to open the dialog.
    /// </summary>
    public ICommand OpenDialogCommand {
        get => _openDialogCommand;
        private set => SetAndRaise<ICommand>(OpenDialogCommandProperty, ref _openDialogCommand, value);
    }

    /// <summary>
    /// Gets the command to close the dialog.
    /// </summary>
    public ICommand CloseDialogCommand {
        get => _closeDialogCommand;
        private set => SetAndRaise<ICommand>(CloseDialogCommandProperty, ref _closeDialogCommand, value);
    }

    /// <summary>
    /// Gets or sets the unique identifier for this dialog host instance.
    /// </summary>
    public string? Identifier {
        get => _identifier;
        set => SetAndRaise(IdentifierProperty, ref _identifier, value);
    }

    /// <summary>
    /// Gets or sets the content to display in the dialog.
    /// </summary>
    public object? DialogContent {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the dialog content.
    /// </summary>
    public IDataTemplate? DialogContentTemplate {
        get => GetValue(DialogContentTemplateProperty);
        set => SetValue(DialogContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush for the overlay.
    /// </summary>
    public IBrush OverlayBackground {
        get => GetValue(OverlayBackgroundProperty);
        set => SetValue(OverlayBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the margin around the dialog content.
    /// </summary>
    public Thickness DialogMargin {
        get => GetValue(DialogMarginProperty);
        set => SetValue(DialogMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dialog is currently open.
    /// </summary>
    public bool IsOpen {
        get => _isOpen;
        set {
            if (SetAndRaise(IsOpenProperty, ref _isOpen, value))
            {
                IsOpenPropertyChangedCallback(this, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether clicking outside the dialog area should close it.
    /// </summary>
    public bool CloseOnClickAway {
        get => _closeOnClickAway;
        set => SetAndRaise(CloseOnClickAwayProperty, ref _closeOnClickAway, value);
    }

    /// <summary>
    /// Gets or sets the parameter passed to close handler when dialog is closed by clicking away.
    /// </summary>
    public object? CloseOnClickAwayParameter {
        get => _closeOnClickAwayParameter;
        set => SetAndRaise(CloseOnClickAwayParameterProperty, ref _closeOnClickAwayParameter, value);
    }

    /// <summary>
    /// Gets or sets whether to disable the opening animation.
    /// </summary>
    public bool DisableOpeningAnimation {
        get => _disableOpeningAnimation;
        set => SetAndRaise(DisableOpeningAnimationProperty, ref _disableOpeningAnimation, value);
    }

    /// <summary>
    /// Allows to override popup positioner
    /// </summary>
    public IDialogPopupPositioner? PopupPositioner {
        get => _popupPositioner;
        set => SetAndRaise(PopupPositionerProperty, ref _popupPositioner, value);
    }
    
    /// <summary>
    /// Gets or sets whether to enable background blur when dialog is opened
    /// </summary>
    public bool BlurBackground {
        get => GetValue(BlurBackgroundProperty);
        set => SetValue(BlurBackgroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets radius of background blur when <see cref="BlurBackground"/> is enabled
    /// </summary>
    public double BlurBackgroundRadius {
        get => GetValue(BlurBackgroundRadiusProperty);
        set => SetValue(BlurBackgroundRadiusProperty, value);
    }

    /// <summary>
    /// Returns a DialogSession for the currently open dialog for managing it programmatically. If no dialog is open, CurrentSession will return null
    /// </summary>
    public DialogSession? CurrentSession { get; private set; }

    /// <summary>
    /// Gets or sets the callback for when the dialog is closing.
    /// </summary>
    public DialogClosingEventHandler? DialogClosingCallback {
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
        => Show(content, (string?)null, openedEventHandler, null);

    /// <summary>
    /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
    /// </summary>
    /// <param name="content">Content to show (can be a control or view model).</param>
    /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
    /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
    public static Task<object?> Show(object content, DialogClosingEventHandler closingEventHandler)
        => Show(content, (string?)null, null, closingEventHandler);

    /// <summary>
    /// Shows a modal dialog. To use, a <see cref="DialogHost"/> instance must be in a visual tree (typically this may be specified towards the root of a Window's XAML).
    /// </summary>
    /// <param name="content">Content to show (can be a control or view model).</param>        
    /// <param name="openedEventHandler">Allows access to opened event which would otherwise have been subscribed to on a instance.</param>
    /// <param name="closingEventHandler">Allows access to closing event which would otherwise have been subscribed to on a instance.</param>
    /// <returns>Task result is the parameter used to close the dialog, typically what is passed to the <see cref="CloseDialogCommand"/> command.</returns>
    public static Task<object?> Show(object content, DialogOpenedEventHandler? openedEventHandler, DialogClosingEventHandler? closingEventHandler)
        => Show(content, (string?)null, openedEventHandler, closingEventHandler);

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
        return GetInstance(dialogIdentifier).ShowCore(content, openedEventHandler, closingEventHandler);
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
        return instance.ShowCore(content, openedEventHandler, closingEventHandler);
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
        var dialogHost = GetInstance(dialogIdentifier);
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
        var dialogHost = GetInstance(dialogIdentifier);
        return dialogHost.CurrentSession;
    }

    /// <summary>
    /// dialog instance exists
    /// </summary>
    /// <param name="dialogIdentifier">of the instance where the dialog should be closed. Typically this will match an identifier set in XAML.</param>
    /// <returns></returns>
    public static bool IsDialogOpen(string? dialogIdentifier) => GetDialogSession(dialogIdentifier)?.IsEnded == false;

    private static DialogHost GetInstance(string? dialogIdentifier) {
        if (_loadedInstances.Count == 0)
            throw new InvalidOperationException("No loaded DialogHost instances.");

        var targets = _loadedInstances.Where(dh => dialogIdentifier == null || Equals(dh.Identifier, dialogIdentifier)).ToList();
        if (targets.Count == 0)
            throw new InvalidOperationException(
                $"No loaded DialogHost have an {nameof(Identifier)} property matching {nameof(dialogIdentifier)} ('{dialogIdentifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException(
                "Multiple viable DialogHosts. Specify a unique Identifier on each DialogHost, especially where multiple Windows are a concern.");

        return targets[0];
    }

    private async Task<object?> ShowCore(object content, DialogOpenedEventHandler? openedEventHandler,
        DialogClosingEventHandler? closingEventHandler) {
        if (IsOpen)
            throw new InvalidOperationException("DialogHost is already open.");

        _dialogTaskCompletionSource = new TaskCompletionSource<object?>();

        if (content != null)
            DialogContent = content;

        _asyncShowOpenedEventHandler = openedEventHandler;
        _asyncShowClosingEventHandler = closingEventHandler;
        IsOpen = true;

        var result = await _dialogTaskCompletionSource.Task;

        _asyncShowOpenedEventHandler = null;
        _asyncShowClosingEventHandler = null;

        return result;
    }

    private static void IsOpenPropertyChangedCallback(DialogHost dialogHost, bool newValue) {
        if (newValue) {
            dialogHost.CurrentSession = new DialogSession(dialogHost);
            dialogHost._restoreFocusDialogClose = TopLevel.GetTopLevel(dialogHost)?.FocusManager?.GetFocusedElement();

            if (dialogHost._overlayPopupHost != null)
                dialogHost._overlayPopupHost.IsOpen = true;

            //multiple ways of calling back that the dialog has opened:
            // * routed event
            // * straight forward dependency property 
            // * handler provided to the async show method
            var dialogOpenedEventArgs = new DialogOpenedEventArgs(dialogHost.CurrentSession, DialogOpenedEvent);
            dialogHost.OnDialogOpened(dialogOpenedEventArgs);
            dialogHost.DialogOpenedCallback?.Invoke(dialogHost, dialogOpenedEventArgs);
            dialogHost._asyncShowOpenedEventHandler?.Invoke(dialogHost, dialogOpenedEventArgs);

            // dialogHost._overlayPopupHost?.ConfigurePosition(dialogHost._root, PlacementMode.AnchorAndGravity, new Point());
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

            if (dialogHost._overlayPopupHost != null)
                dialogHost._overlayPopupHost.IsOpen = false;
            //NB: _dialogTaskCompletionSource is only set in the case where the dialog is shown with Show
            dialogHost._dialogTaskCompletionSource?.TrySetResult(closeParameter);

            dialogHost._restoreFocusDialogClose?.Focus();
        }

        dialogHost.RaiseCommandsCanExecuteChanged();
    }

    /// <summary>
    /// Raises the can execute changed on open and close commands
    /// </summary>
    protected void RaiseCommandsCanExecuteChanged() {
        (_openDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
        (_closeDialogCommand as DialogHostCommandImpl)?.OnCanExecuteChanged();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        _templateDisposables?.Dispose();

        _root = e.NameScope.Find<Panel>(DialogHostRoot) 
                ?? throw new InvalidOperationException($"No Panel with name {DialogHostRoot} found. " +
                                                       $"Did you add the styles as stated in getting started?");
        _overlayPopupHost = new DialogOverlayPopupHost(_root) {
            Content = DialogContent, ContentTemplate = DialogContentTemplate, Template = PopupTemplate,
            Padding = DialogMargin, ClipToBounds = false, DisableOpeningAnimation = DisableOpeningAnimation,
            PopupPositioner = PopupPositioner
        };

        if (IsOpen) {
            _overlayPopupHost.IsOpen = true;
            // _overlayPopupHost?.ConfigurePosition(_root, PlacementMode.AnchorAndGravity, new Point());
        }

        _templateDisposables = new CompositeDisposable() {
            // this.GetObservable(BoundsProperty)
            // .Subscribe(rect => _overlayPopupHost?.ConfigurePosition(_root, PlacementMode.AnchorAndGravity, new Point())),
            _overlayPopupHost!.Bind(DisableOpeningAnimationProperty, this.GetBindingObservable(DisableOpeningAnimationProperty)),
            _overlayPopupHost!.Bind(ContentProperty, this.GetBindingObservable(DialogContentProperty)),
            _overlayPopupHost!.Bind(ContentTemplateProperty, this.GetBindingObservable(DialogContentTemplateProperty)),
            _overlayPopupHost!.Bind(TemplateProperty, this.GetBindingObservable(PopupTemplateProperty)),
            _overlayPopupHost!.Bind(PaddingProperty, this.GetBindingObservable(DialogMarginProperty)),
            _overlayPopupHost!.Bind(PopupPositionerProperty, this.GetBindingObservable(PopupPositionerProperty)),
            e.NameScope.Find<Rectangle>(ContentCoverName)?.AddDisposableHandler(PointerReleasedEvent, ContentCoverGrid_OnPointerReleased) ?? EmptyDisposable.Instance
        };
        base.OnApplyTemplate(e);
    }

    private void ContentCoverGrid_OnPointerReleased(object sender, PointerReleasedEventArgs e) {
        if (CloseOnClickAway && CurrentSession != null) {
            InternalClose(CloseOnClickAwayParameter);
        }
    }

    /// <summary>
    /// Called when dialog is opened
    /// </summary>
    /// <param name="dialogOpenedEventArgs">Dialog opened event arguments</param>
    protected virtual void OnDialogOpened(DialogOpenedEventArgs dialogOpenedEventArgs) => RaiseEvent(dialogOpenedEventArgs);

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

    /// <summary>
    /// Called when dialog is closed
    /// </summary>
    /// <param name="eventArgs">Dialog closed event arguments</param>
    protected virtual void OnDialogClosing(DialogClosingEventArgs eventArgs) => RaiseEvent(eventArgs);

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

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        _loadedInstances.Add(this);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        _loadedInstances.Remove(this);
    }
}