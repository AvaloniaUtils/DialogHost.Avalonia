using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Avalonia.DialogHost {
    // public class DialogHostPopup : Control {
    //     public static readonly StyledProperty<Control?> PlacementTargetProperty =
    //         Popup.PlacementTargetProperty.AddOwner<DialogHostPopup>();
    //
    //     public Control? PlacementTarget {
    //         get => GetValue(PlacementTargetProperty);
    //         set => SetValue(PlacementTargetProperty, value);
    //     }
    //
    //     public static readonly StyledProperty<Control?> RootControlProperty =
    //         AvaloniaProperty.Register<DialogHostPopup, Control?>(nameof(RootControl));
    //
    //     public Control? RootControl {
    //         get => GetValue(RootControlProperty);
    //         set => SetValue(RootControlProperty, value);
    //     }
    //
    //     public static readonly StyledProperty<DialogPopupModes> DialogPopupModeProperty =
    //         AvaloniaProperty.Register<DialogHostPopup, DialogPopupModes>(nameof(DialogPopupMode), DialogPopupModes.Auto);
    //
    //     public DialogPopupModes DialogPopupMode {
    //         get => GetValue(DialogPopupModeProperty);
    //         set => SetValue(DialogPopupModeProperty, value);
    //     }
    //
    //     public IAvaloniaDependencyResolver? DependencyResolver { get; set; }
    //
    //     public new void Open() {
    //         var popupHost = GetPopupHost();
    //     }
    //
    //     private IPopupHost GetPopupHost() {
    //         var topLevel = DialogPopupMode switch {
    //             DialogPopupModes.ForceWindow  => PlacementTarget.GetVisualRoot() as TopLevel,
    //             DialogPopupModes.Auto         => PlacementTarget.GetVisualParent() as TopLevel,
    //             DialogPopupModes.ForceOverlay => null,
    //             _                             => throw new ArgumentOutOfRangeException()
    //         };
    //         var platform = topLevel?.PlatformImpl?.CreatePopup();
    //         if (platform != null)
    //             return new PopupRoot(topLevel, platform, DependencyResolver);
    //
    //         var overlayLayer = OverlayLayer.GetOverlayLayer(target);
    //         if (overlayLayer == null)
    //             throw new InvalidOperationException(
    //                 "Unable to create IPopupImpl and no overlay layer is found for the target control");
    //
    //         return new OverlayPopupHost(overlayLayer);
    //     }
    //
    //     public enum DialogPopupModes {
    //         ForceWindow,
    //         Auto,
    //         ForceOverlay
    //     }
    // }
}