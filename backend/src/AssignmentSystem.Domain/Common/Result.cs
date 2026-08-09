namespace AssignmentSystem.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Forbidden,
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ErrorType errorType = ErrorType.Validation)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("A successful result cannot have an error.");
        }

        if (!isSuccess && error is null)
        {
            throw new InvalidOperationException("A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true, null);
    public static Result Failure(string error, ErrorType errorType = ErrorType.Validation) => new(false, error, errorType);
    public static Result NotFound(string error) => new(false, error, ErrorType.NotFound);
    public static Result Forbidden(string error) => new(false, error, ErrorType.Forbidden);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error, ErrorType errorType = ErrorType.Validation) => new(default, false, error, errorType);
    public static Result<T> NotFound<T>(string error) => new(default, false, error, ErrorType.NotFound);
    public static Result<T> Forbidden<T>(string error) => new(default, false, error, ErrorType.Forbidden);
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, string? error, ErrorType errorType = ErrorType.Validation)
        : base(isSuccess, error, errorType)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");
}
