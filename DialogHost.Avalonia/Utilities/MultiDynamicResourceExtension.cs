using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using Avalonia.Styling;

namespace DialogHostAvalonia.Utilities
{
    /// <summary>
    /// Allows binding to several different DynamicResources
    /// </summary>
    // TODO: Replace it with proper implementation when https://github.com/AvaloniaUI/Avalonia/issues/15270 will be resolved
    internal class MultiDynamicResourceExtension : MarkupExtension
    {
        public IBrush DefaultBrush { get; set; }

        public object ResourceKeys { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = serviceProvider.GetService<IProvideValueTarget>();
            if (provideValueTarget is null)
            {
                throw new InvalidOperationException(
                    "Can't resolve IProvideValueTarget from Avalonia service provider.");
            }

            var setter = (Setter)provideValueTarget.TargetObject;
            var targetProperty = setter.Property;

            if (ResourceKeys is not string resourceKey)
            {
                throw new InvalidOperationException(
                    "ResourceKeys should be string with ; delimeter");
            }

            var resourceKeys = resourceKey.Split(';');
            var source = resourceKeys
                .Select(key => Application.Current!
                    .GetResourceObservable(key, GetConverter(targetProperty)));
            var testObservable = new MultiDynamicResourceObservable(source, DefaultBrush);
            return testObservable.ToBinding();
        }

        private static Func<object?, object?>? GetConverter(AvaloniaProperty? targetProperty)
        {
            if (targetProperty?.PropertyType == typeof(IBrush))
            {
                return x => ColorToBrushConverter.Convert(x, typeof(IBrush));
            }

            return null;
        }

        public sealed class MultiDynamicResourceObservable : IObservable<object?>, IDisposable
        {
            public MultiDynamicResourceObservable(IEnumerable<IObservable<object?>> observables, object? defaultValue)
            {
                var dictionary = observables.ToDictionary(observable => observable, _ => (object?)null);
                var disposables = dictionary.Keys
                    .Select(obs => obs
                        .Subscribe(o =>
                        {
                            dictionary[obs] = o;
                            var newValue =
                                dictionary.Values.LastOrDefault(o1 => o1 is not UnsetValueType && o1 is not null) ??
                                defaultValue;
                            _subject.OnNext(newValue);
                        }))
                    .ToList();
                _disposable = new CompositeDisposable(disposables);
            }

            private readonly CompositeDisposable _disposable;
            private readonly BehaviorSubject<object?> _subject = new(null);

            /// <inheritdoc />
            public IDisposable Subscribe(IObserver<object?> observer)
            {
                return _subject.Subscribe(observer);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _disposable.Dispose();
                _subject.Dispose();
            }
        }
    }
}