using System;
using System.Threading.Tasks;

public static class TupleEitherExtensions
{
    public static TResult Match<T, TResult>(this (T? Result, Failure? Error) either, Func<T, TResult> onSuccess, Func<Failure, TResult> onFailure)
    {
        if (either.Error != null) return onFailure(either.Error);
        return onSuccess(either.Result!);
    }

    public static async Task<TResult> Match<T, TResult>(this (T? Result, Failure? Error) either, Func<T, Task<TResult>> onSuccess, Func<Failure, Task<TResult>> onFailure)
    {
        if (either.Error != null) return await onFailure(either.Error).ConfigureAwait(false);
        return await onSuccess(either.Result!).ConfigureAwait(false);
    }

    public static void IfLeft<T>(this (T? Result, Failure? Error) either, Action<Failure> onLeft)
    {
        if (either.Error != null) onLeft(either.Error);
    }

    public static Task IfLeftAsync<T>(this (T? Result, Failure? Error) either, Func<Failure, Task> onLeft)
    {
        return either.Error != null ? onLeft(either.Error) : Task.CompletedTask;
    }
}
