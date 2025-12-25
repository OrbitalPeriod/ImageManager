namespace ImageManager.Data.Helpers;

/// <summary>
/// Monad in either 1 of 2 states, if IsOk is true, T is a valid initialized value. else, E is initialized and valid.
/// </summary>
/// <typeparam name="T">The Ok value, this is returned in a successfull operation</typeparam>
/// <typeparam name="E">The Err value, this is returned in an unsuccessfull operation</typeparam>
public class Result<T, E>
{
    private T? OkValue { get; init; }
    private E? ErrorValue { get; init; }
    /// <summary>
    /// if IsOk is true, T is a valid initialized value. else, E is initialized and valid.
    /// </summary>
    public bool IsOk { get; }

    private Result(T? ok, E? error, bool isOk)
    {
        OkValue = ok;
        ErrorValue = error;
        IsOk = isOk;
    }

    /// <summary>
    /// Initializes the class with value as T to the OK variant.
    /// </summary>
    /// <param name="value">Value to be stored in T</param>
    /// <returns></returns>
    public static Result<T, E> Ok(T value) => new(value, default, true);
    /// <summary>
    /// Initializes the class with value as E to the Err variant.
    /// </summary>
    /// <param name="value">Value to be stored in E</param>
    /// <returns></returns>
    public static Result<T, E> Err(E value) => new(default, value, false);

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns Ok if the variant is Ok</returns>
    /// <exception cref="UnwrapException">If the variant if Err</exception>
    public T Unwrap()
    {
        if (!IsOk) throw new UnwrapException();
        else return (T)OkValue!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg">message for exception f variant is Err</param>
    /// <returns>Returns Ok if the variant is Ok</returns>
    /// <exception cref="UnwrapException">If the variant if Err</exception>
    public T Expect(string msg)
    {
        if (!IsOk) throw new UnwrapException(msg);
        else return (T)OkValue!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="or">Value to be returned if variant is Err</param>
    /// <returns>Returns Ok variant if Ok, Err variant of Err</returns>
    public T UnwrapOr(T or)
    {
        if (!IsOk) return or;
        else return (T)OkValue!;
    }

    /// <summary>
    /// Returns the value stores in T without checking if this is valid.
    /// </summary>
    /// <returns>IsOk casted to a T</returns>
    public T UnwrapUnchecked()
    {
        return (T)OkValue!;
    }

    /// <summary>
    /// Returns the value in E without
    /// </summary>
    /// <returns>Returns E if Err variant</returns>
    /// <exception cref="UnwrapException">Throws exception if variant is Ok</exception>
    public E UnwrapError()
    {
        if (IsOk) throw new UnwrapException();
        return (E)ErrorValue!;
    }

    /// <summary>
    /// Maps value T over function mapper
    /// </summary>
    /// <param name="mapper"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Result<U, E> Map<U>(Func<T, U> mapper)
    {
        if (IsOk)
            return Result<U, E>.Ok(mapper((T)OkValue!));
        else
            return Result<U, E>.Err((E)ErrorValue!);
    }

    /// <summary>
    /// Maps the error value with the supplied function.
    /// </summary>
    /// <param name="mapper">A function that turns an <see cref="E"/> into a new error type.</param>
    /// <typeparam name="F">The target error type after mapping.</typeparam>
    /// <returns>A new <see cref="Result{T,F}"/> – the Ok side stays the same, the Err side is transformed.</returns>
    public Result<T, F> MapErr<F>(Func<E, F> mapper)
    {
        if (IsOk)
            return Result<T, F>.Ok((T)OkValue!);

        return Result<T, F>.Err(mapper((E)ErrorValue!));
    }
}

public class UnwrapException : Exception
{
    public UnwrapException() { }
    public UnwrapException(string message) : base(message) { }
    public UnwrapException(string message, Exception inner) : base(message, inner) { }
}
