using System;
using System.Linq;
using Avalonia.Markup.Xaml.XamlIl.Runtime;

namespace DialogHostAvalonia.Utilities {
    /// <remarks>
    /// Copied from AvaloniaUI
    /// </remarks>
    internal static class Extensions
    {
        public static T? GetService<T>(this IServiceProvider sp) => (T?)sp?.GetService(typeof(T));
        
        public static T? GetFirstParent<T>(this IServiceProvider ctx) where T : class 
            => ctx.GetService<IAvaloniaXamlIlParentStackProvider>()!.Parents.OfType<T>().FirstOrDefault();

        public static bool IsInControlTemplate(this IServiceProvider sp) => sp.GetService<IAvaloniaXamlIlControlTemplateProvider>() != null;
    }
}