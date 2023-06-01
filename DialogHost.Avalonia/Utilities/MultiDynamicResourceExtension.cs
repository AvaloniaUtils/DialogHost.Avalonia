using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;

namespace DialogHostAvalonia.Utilities {
    /// <summary>
    /// Allows to bind to several different DynamicResources
    /// </summary>
    internal class MultiDynamicResourceExtension : Binding, IBinding {
        public IBrush DefaultBrush { get; set; }

        public object ResourceKeys { get; set; }
        
        public IBinding ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        InstancedBinding? IBinding.Initiate(
            AvaloniaObject target,
            AvaloniaProperty? targetProperty,
            object? anchor,
            bool enableDataValidation)
        {
            if (ResourceKeys is not string resourceKey) {
                return null;
            }

            var resourceKeys = resourceKey.Split(';');
            
            var control = target as IResourceHost ?? DefaultAnchor?.Target as IResourceHost;

            if (control != null)
            {
                var source = resourceKeys.Select(key => control.GetResourceObservable(key, GetConverter(targetProperty)));
                var testObservable = new MultiDynamicResourceObservable(source, DefaultBrush);
                return InstancedBinding.OneWay(testObservable, Priority);
            }
            if (DefaultAnchor?.Target is IResourceProvider resourceProvider)
            {
                var source = resourceKeys.Select(key => resourceProvider.GetResourceObservable(key, GetConverter(targetProperty)));
                var testObservable = new MultiDynamicResourceObservable(source, DefaultBrush);
                return InstancedBinding.OneWay(testObservable, Priority);
            }

            return null;
        }

        private static Func<object?, object?>? GetConverter(AvaloniaProperty? targetProperty)
        {
            if (targetProperty?.PropertyType == typeof(IBrush))
            {
                return x => ColorToBrushConverter.Convert(x, typeof(IBrush));
            }

            return null;
        }
        
        public sealed class MultiDynamicResourceObservable : IObservable<object?>, IDisposable {
            public MultiDynamicResourceObservable(IEnumerable<IObservable<object?>> observables, object? defaultValue) {
                var dictionary = observables.ToDictionary(observable => observable, _ => (object?)null);
                _disposable = new CompositeDisposable(dictionary.Keys.Select(obs => obs.Subscribe(o => {
                    dictionary[obs] = o;
                    _subject.OnNext(dictionary.Values.LastOrDefault(o1 => o1 is not UnsetValueType && o1 is not null) ?? defaultValue);
                })));
            }
            private readonly CompositeDisposable _disposable;
            private readonly BehaviorSubject<object?> _subject = new(null);
            /// <inheritdoc />
            public IDisposable Subscribe(IObserver<object?> observer) {
                return _subject.Subscribe(observer);
            }
            /// <inheritdoc />
            public void Dispose() {
                _disposable.Dispose();
                _subject.Dispose();
            }
        }
    }
}