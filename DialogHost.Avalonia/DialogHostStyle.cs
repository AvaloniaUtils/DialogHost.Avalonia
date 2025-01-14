﻿using Avalonia;
using Avalonia.Media;

namespace DialogHostAvalonia;

/// <summary>
/// Contains properties for <see cref="DialogHost"/> default theme
/// </summary>
public static class DialogHostStyle {
    /// <summary>
    /// Controls CornerRadius DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.RegisterAttached<DialogHost, CornerRadius>("CornerRadius", typeof(DialogHostStyle));

    /// <summary>
    /// Controls ClipToBounds DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static readonly AttachedProperty<bool> ClipToBoundsProperty =
        AvaloniaProperty.RegisterAttached<DialogHost, bool>("ClipToBounds", typeof(DialogHostStyle));

    /// <summary>
    /// Controls BorderBrush DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static readonly AttachedProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.RegisterAttached<DialogHost, IBrush>("BorderBrush", typeof(DialogHostStyle));

    /// <summary>
    /// Controls BorderThickness DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static readonly AttachedProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DialogHost, Thickness>("BorderThickness", typeof(DialogHostStyle));

    /// <summary>
    /// Get CornerRadius in DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static CornerRadius GetCornerRadius(DialogHost element)
    {
        return element.GetValue(CornerRadiusProperty);
    }

    /// <summary>
    /// Get ClipToBounds in DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static bool GetClipToBounds(DialogHost element)
    {
        return element.GetValue(ClipToBoundsProperty);
    }

    /// <summary>
    /// Set CornerRadius in DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static void SetCornerRadius(DialogHost element, CornerRadius value)
    {
        element.SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Set ClipToBounds in DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static void SetClipToBounds(DialogHost element, bool value)
    {
        element.SetValue(ClipToBoundsProperty, value);
    }

    /// <summary>
    /// Set BorderBrush in DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static void SetBorderBrush(DialogHost element, IBrush value)
    {
        element.SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Set BorderThickness in DialogHost's popup background.
    /// Works only for default DialogHost theme!
    /// </summary>
    public static void SetBorderThickness(DialogHost element, Thickness value)
    {
        element.SetValue(BorderThicknessProperty, value);
    }
}