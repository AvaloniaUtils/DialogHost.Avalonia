using Avalonia;

namespace DialogHost
{
    public class DialogHostStyle
    {
        /// <summary>
        /// Controls CornerRadius DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.RegisterAttached<DialogHostStyle, DialogHost, CornerRadius>("CornerRadius");

        /// <summary>
        /// Controls ClipToBounds DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static readonly AttachedProperty<bool> ClipToBoundsProperty =
            AvaloniaProperty.RegisterAttached<DialogHostStyle, DialogHost, bool>("ClipToBounds");

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
    }
}