using Avalonia;

namespace DialogHostAvalonia.Positioners
{
    /// <summary>
    /// Implement this for your positioner if it constrains the popup, e.g. margin
    /// </summary>
    public interface IDialogPopupPositionerConstrainable
    {
        /// <summary>
        /// Constrain the space available for popup
        /// </summary>
        /// <param name="availableSize">Initial size</param>
        /// <returns>Constrained size</returns>
        public Size Constrain(Size availableSize);
    }
}