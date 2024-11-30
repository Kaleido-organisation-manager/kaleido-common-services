using Kaleido.Common.Services.Grpc.Constants;

namespace Kaleido.Common.Services.Grpc.Models;

public readonly struct ManagerResponse<T> where T : class
{
    public readonly ManagerResponseState State { get; }
    public readonly T? Result { get; }

    private ManagerResponse(ManagerResponseState state, T? result = default)
    {
        State = state;
        Result = result;
    }

    public static ManagerResponse<T> Success(T result) => new(ManagerResponseState.Success, result);
    public static ManagerResponse<T> NotFound() => new(ManagerResponseState.NotFound);
    public static ManagerResponse<T> NotModified() => new(ManagerResponseState.NotModified);
    public static ManagerResponse<T> Invalid() => new(ManagerResponseState.Invalid);
}

public readonly struct ManagerResponse<T1, T2> where T1 : class where T2 : class
{
    public readonly ManagerResponseState State { get; }
    public readonly T1? Result1 { get; }
    public readonly T2? Result2 { get; }

    private ManagerResponse(ManagerResponseState state, T1? result1 = default, T2? result2 = default)
    {
        State = state;
        Result1 = result1;
        Result2 = result2;
    }

    public static ManagerResponse<T1, T2> Success(T1 result1, T2 result2) =>
        new(ManagerResponseState.Success, result1, result2);
    public static ManagerResponse<T1, T2> NotFound() => new(ManagerResponseState.NotFound);
    public static ManagerResponse<T1, T2> NotModified() => new(ManagerResponseState.NotModified);
    public static ManagerResponse<T1, T2> Invalid() => new(ManagerResponseState.Invalid);
}