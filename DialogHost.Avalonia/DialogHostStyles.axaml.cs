using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace DialogHostAvalonia;

/// <summary>
/// Provides styles set for using <see cref="DialogHost"/>
/// </summary>
/// <remarks>
/// Add it to your App.axaml file:
/// <code>
/// &lt;Application ...
///             xmlns:dialogHostAvalonia="clr-namespace:DialogHostAvalonia;assembly=DialogHost.Avalonia"&gt;
/// 
///    &lt;Application.Styles&gt;
///        ...
///        &lt;dialogHostAvalonia:DialogHostStyles/&gt;
///    &lt;/Application.Styles&gt;
/// &lt;/Application&gt;
/// </code>
/// </remarks>
public class DialogHostStyles : Styles, IResourceNode {
    /// <inheritdoc />
    public DialogHostStyles() {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public DialogHostStyles(IResourceHost owner) : base(owner) {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Lists of resource keys which will be used as defaults for DialogHost.Background property
    /// </summary>
    public static IReadOnlyList<string> BackgroundColorKeys { get; } =
        ["SystemControlBackgroundAltHighBrush", "MaterialDesignPaper", "MaterialPaperBrush"];

    /// <summary>
    /// Lists of resource keys which will be used as defaults for DialogHost.OverlayBackground property
    /// </summary>
    public static IReadOnlyList<string> OverlayBackgroundColorKeys { get; } =
        ["SystemControlPageBackgroundChromeLowBrush"];

    bool IResourceNode.HasResources => true;

    /// <summary>
    /// Tries to find a resource within the object.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="theme">Theme used to select theme dictionary.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key,
    /// if the key is found; otherwise, null.
    /// </param>
    /// <returns>
    /// True if the resource is found, otherwise false.
    /// </returns>
    bool IResourceNode.TryGetResource(object key, ThemeVariant? theme, out object? value) {
        if (key is "DialogHostBackgroundMixinBrush") {
            foreach (var colorKey in BackgroundColorKeys) {
                if (Application.Current!.TryGetResource(colorKey, theme, out value)) {
                    return true;
                }
            }

            value = Brushes.Black;
            return true;
        }

        if (key is "DialogHostOverlayBackgroundMixinBrush") {
            foreach (var colorKey in OverlayBackgroundColorKeys) {
                if (Application.Current!.TryGetResource(colorKey, theme, out value)) {
                    return true;
                }
            }

            value = Brushes.Black;
            return true;
        }

        return base.TryGetResource(key, theme, out value);
    }
}