using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Models.Extensions;

public static class ManagerResponseExtensions
{
    public static ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>> ToManagerResponse<TEntity, TRevision>(
        this EntityLifeCycleResult<TEntity, TRevision>? result)
        where TEntity : BaseEntity, new()
        where TRevision : BaseRevisionEntity, new()
    {
        return result == null
            ? ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>>.NotFound()
            : ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>>.Success(result);
    }

    public static ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>> ToManagerResponse<TEntity, TRevision>(
        this EntityLifeCycleResult<TEntity, TRevision>? result,
        bool modified)
        where TEntity : BaseEntity, new()
        where TRevision : BaseRevisionEntity, new()
    {
        if (result == null)
            return ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>>.NotFound();

        return modified
            ? ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>>.Success(result)
            : ManagerResponse<EntityLifeCycleResult<TEntity, TRevision>>.NotModified();
    }

    public static ManagerResponse<TResult1, TResult2> ToManagerResponse<TResult1, TResult2>(
        this (TResult1? Result1, TResult2? Result2) results)
        where TResult1 : class
        where TResult2 : class
    {
        return results.Result1 == null || results.Result2 == null
            ? ManagerResponse<TResult1, TResult2>.NotFound()
            : ManagerResponse<TResult1, TResult2>.Success(results.Result1, results.Result2);
    }

    public static ManagerResponse<TResult1, TResult2> ToManagerResponse<TResult1, TResult2>(
        this (TResult1? Result1, TResult2? Result2) results,
        bool modified)
        where TResult1 : class
        where TResult2 : class
    {
        if (results.Result1 == null || results.Result2 == null)
            return ManagerResponse<TResult1, TResult2>.NotFound();

        return modified
            ? ManagerResponse<TResult1, TResult2>.Success(results.Result1, results.Result2)
            : ManagerResponse<TResult1, TResult2>.NotModified();
    }
}