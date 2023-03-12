using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace DialogHostAvalonia {
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
    public class DialogHostStyles : Styles {
        /// <inheritdoc />
        public DialogHostStyles() {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc />
        public DialogHostStyles(IResourceHost owner) : base(owner) {
            AvaloniaXamlLoader.Load(this);
        }
    }
}