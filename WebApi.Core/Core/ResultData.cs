namespace WebApi.Core;

public abstract record ResultData<T>;
public sealed record Success<T>(T Data) : ResultData<T>;
public sealed record Error<T>(Exception Exception) : ResultData<T>;