using Avalonia;

namespace DialogHostAvalonia {
    /// <summary>
    /// Contain properties to control default <see cref="DialogHost"/> theme behaviors.
    /// This may be shouldn't work with other themes 
    /// </summary>
    public static class DialogHostDefaultStyle {
        /// <summary>
        /// Controls CornerRadius DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.RegisterAttached<DialogHost, CornerRadius>("CornerRadius", typeof(DialogHostDefaultStyle));

        /// <summary>
        /// Controls ClipToBounds DialogHost's popup background.
        /// Works only for default DialogHost theme!
        /// </summary>
        public static readonly AttachedProperty<bool> ClipToBoundsProperty =
            AvaloniaProperty.RegisterAttached<DialogHost, bool>("ClipToBounds", typeof(DialogHostDefaultStyle));

        /// <summary>
        /// Controls should be popup animations will be set to default ones.
        /// If you set <see cref="DialogHost.OpeningAnimation"/> or <see cref="DialogHost.ClosingAnimation"/> by hands, it will ignore this property
        /// </summary>
        public static readonly AttachedProperty<bool> DisableDefaultPopupAnimationsProperty = 
            AvaloniaProperty.RegisterAttached<DialogHost, bool>("DisablePopupAnimations", typeof(DialogHostDefaultStyle));

        /// <summary>
        /// Controls should be content cover grid animations disabled.
        /// </summary>
        public static readonly AttachedProperty<bool> DisableContentCoverGridAnimationsProperty = 
            AvaloniaProperty.RegisterAttached<DialogHost, bool>("DisableContentCoverGridAnimations", typeof(DialogHostDefaultStyle));

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
        /// Controls should be popup animations will be set to default ones.
        /// If you set <see cref="DialogHost.OpeningAnimation"/> or <see cref="DialogHost.ClosingAnimation"/> by hands, it will ignore this property
        /// </summary>
        public static bool GetDisablePopupAnimations(DialogHost element) {
            return element.GetValue(DisableDefaultPopupAnimationsProperty);
        }

        /// <summary>
        /// Controls should be popup animations will be set to default ones.
        /// If you set <see cref="DialogHost.OpeningAnimation"/> or <see cref="DialogHost.ClosingAnimation"/> by hands, it will ignore this property
        /// </summary>
        public static void SetDisablePopupAnimations(DialogHost element, bool value) {
            element.SetValue(DisableDefaultPopupAnimationsProperty, value);
        }
        
        /// <summary>
        /// Controls should be content cover grid animations disabled.
        /// If set to <c>null</c> - will follow <see cref="DisableDefaultPopupAnimationsProperty"/> value.
        /// </summary>
        public static bool GetDisableContentCoverGridAnimations(DialogHost element) {
            return element.GetValue(DisableContentCoverGridAnimationsProperty);
        }

        /// <summary>
        /// Controls should be content cover grid animations disabled.
        /// If set to <c>null</c> - will follow <see cref="DisableDefaultPopupAnimationsProperty"/> value.
        /// </summary>
        public static void SetDisableContentCoverGridAnimations(DialogHost element, bool value) {
            element.SetValue(DisableContentCoverGridAnimationsProperty, value);
        }
    }
}