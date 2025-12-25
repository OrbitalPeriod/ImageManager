namespace ImageManager.Data.Helpers;

/// <summary>
/// Monad in either 1 of 2 states: a value of type, or an empty state.
/// </summary>
public sealed class Option<T>
{
    /// <summary>
    /// The wrapped value when the variant is Some.
    /// </summary>
    private T? Value { get; init; }

    /// <summary>
    /// True if this instance represents a Some(T), false for None.
    /// </summary>
    public bool IsSome { get; }
    public bool IsNone => !IsSome;

    // Private ctor – use the static constructors instead
    private Option(T? value, bool isSome)
    {
        Value = value;
        IsSome = isSome;
    }

    #region Constructors

    /// <summary>
    /// Create a Some containing the given value.
    /// </summary>
    public static Option<T> Some(T value) => new(value, true);

    /// <summary>
    /// Create an empty None.
    /// </summary>
    public static Option<T> None() => new(default, false);

    #endregion

    #region Unwrapping helpers

    /// <summary>
    /// Return the wrapped value if this is Some; otherwise throw <see cref="UnwrapException"/>.
    /// </summary>
    public T Unwrap()
    {
        if (!IsSome) throw new UnwrapException();
        return (T)Value!;
    }

    /// <summary>
    /// Return the wrapped value if this is Some; otherwise throw <see cref="UnwrapException"/> with the supplied message.
    /// </summary>
    public T Expect(string msg)
    {
        if (!IsSome) throw new UnwrapException(msg);
        return (T)Value!;
    }

    /// <summary>
    /// Return the wrapped value if this is Some; otherwise return the provided default.
    /// </summary>
    public T UnwrapOr(T @default) => IsSome ? (T)Value! : @default;

    /// <summary>
    /// Return the wrapped value without any safety checks. Use only when you know the variant is Some.
    /// </summary>
    public T UnwrapUnchecked() => (T)Value!;

    #endregion

    #region Mapping & other helpers

    /// <summary>
    /// Apply a function to the wrapped value if this is Some; otherwise propagate None.
    /// </summary>
    public Option<U> Map<U>(Func<T, U> mapper)
        where U : class
    {
        return IsSome ? Option<U>.Some(mapper((T)Value!)) : Option<U>.None();
    }

    /// <summary>
    /// Apply a function that returns an Option to the wrapped value if this is Some; otherwise propagate None.
    /// </summary>
    public Option<U> FlatMap<U>(Func<T, Option<U>> mapper)
        where U : class
    {
        return IsSome ? mapper((T)Value!) : Option<U>.None();
    }

    #endregion

    // Optional: a nice string representation for debugging
    public override string ToString() =>
        IsSome ? $"Some({Value})" : "None";
}
