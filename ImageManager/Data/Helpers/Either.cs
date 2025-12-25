namespace ImageManager.Data.Helpers;

/// <summary>
/// Monad representing a value that is either Left (L) or Right (R).
/// </summary>
/// <typeparam name="L">Type of the Left value.</typeparam>
/// <typeparam name="R">Type of the Right value.</typeparam>
public sealed class Either<L, R>
{
    private L? LeftValue { get; init; }
    private R? RightValue { get; init; }

    /// <summary>
    /// True when the instance holds a Right value.
    /// </summary>
    public bool IsRight { get; }

    /// <summary>
    /// True when the instance holds a Left value.
    /// </summary>
    public bool IsLeft => !IsRight;

    private Either(L? left, R? right, bool isRight)
    {
        LeftValue = left;
        RightValue = right;
        IsRight = isRight;
    }

    #region Constructors

    /// <summary>
    /// Creates an instance containing a Right value.
    /// </summary>
    public static Either<L, R> Right(R value) => new(default, value, true);

    /// <summary>
    /// Creates an instance containing a Left value.
    /// </summary>
    public static Either<L, R> Left(L value) => new(value, default, false);

    #endregion

    #region Unwrap / Expect / Or

    /// <summary>
    /// Returns the Right value if present; otherwise throws <see cref="UnwrapException"/>.
    /// </summary>
    public R UnwrapRight()
    {
        if (!IsRight) throw new UnwrapException();
        return (R)RightValue!;
    }

    /// <summary>
    /// Returns the Right value if present; otherwise throws <see cref="UnwrapException"/> with supplied message.
    /// </summary>
    public R ExpectRight(string msg)
    {
        if (!IsRight) throw new UnwrapException(msg);
        return (R)RightValue!;
    }

    /// <summary>
    /// Returns the Left value if present; otherwise throws <see cref="UnwrapException"/>.
    /// </summary>
    public L UnwrapLeft()
    {
        if (!IsLeft) throw new UnwrapException();
        return (L)LeftValue!;
    }

    /// <summary>
    /// Returns the Left value if present; otherwise throws <see cref="UnwrapException"/> with supplied message.
    /// </summary>
    public L ExpectLeft(string msg)
    {
        if (!IsLeft) throw new UnwrapException(msg);
        return (L)LeftValue!;
    }

    /// <summary>
    /// Returns the Right value if present; otherwise returns <paramref name="or"/>.
    /// </summary>
    public R UnwrapRightOr(R or)
    {
        return IsRight ? (R)RightValue! : or;
    }

    /// <summary>
    /// Returns the Left value if present; otherwise returns <paramref name="or"/>.
    /// </summary>
    public L UnwrapLeftOr(L or)
    {
        return IsLeft ? (L)LeftValue! : or;
    }

    #endregion

    #region Mapping / Folding

    /// <summary>
    /// Maps the Right value with <paramref name="mapper"/> if present; otherwise propagates the Left unchanged.
    /// </summary>
    public Either<L, U> MapRight<U>(Func<R, U> mapper)
    {
        return IsRight
            ? Either<L, U>.Right(mapper((R)RightValue!))
            : Either<L, U>.Left((L)LeftValue!);
    }

    /// <summary>
    /// Maps the Left value with <paramref name="mapper"/> if present; otherwise propagates the Right unchanged.
    /// </summary>
    public Either<V, R> MapLeft<V>(Func<L, V> mapper)
    {
        return IsLeft
            ? Either<V, R>.Left(mapper((L)LeftValue!))
            : Either<V, R>.Right((R)RightValue!);
    }

    /// <summary>
    /// Performs a side‑effect on the Right value if present.
    /// </summary>
    public void IfRight(Action<R> action)
    {
        if (IsRight) action((R)RightValue!);
    }

    /// <summary>
    /// Performs a side‑effect on the Left value if present.
    /// </summary>
    public void IfLeft(Action<L> action)
    {
        if (IsLeft) action((L)LeftValue!);
    }

    /// <summary>
    /// Folds the Either into a single value by applying either <paramref name="leftMapper"/> or <paramref name="rightMapper"/>.
    /// </summary>
    public T Fold<T>(Func<L, T> leftMapper, Func<R, T> rightMapper)
    {
        return IsLeft
            ? leftMapper((L)LeftValue!)
            : rightMapper((R)RightValue!);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Returns a string representation of the Either.
    /// </summary>
    public override string ToString()
    {
        return IsRight
            ? $"Either.Right({RightValue})"
            : $"Either.Left({LeftValue})";
    }

    #endregion
}

