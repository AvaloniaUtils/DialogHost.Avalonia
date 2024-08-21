using System;

namespace DialogHostAvalonia.Utilities;

/// <summary>
/// Represents an object that is both an observable sequence as well as an observer.
/// </summary>
/// <typeparam name="TSource">
/// The type of the elements received by the subject.
/// This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.
/// </typeparam>
/// <typeparam name="TResult">
/// The type of the elements produced by the subject.
/// This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.
/// </typeparam>
public interface ISubject<in TSource, out TResult> : IObserver<TSource>, IObservable<TResult>
{
}

/// <summary>
/// Represents an object that is both an observable sequence as well as an observer.
/// </summary>
/// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
public interface ISubject<T> : ISubject<T, T>
{
}