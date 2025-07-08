namespace MCP.Server.Common;

public record OperationResult<T> : OperationResult
{
    public T? Data { get; }

    private OperationResult(bool isSuccess, T? data, string? errorMessage) : base(isSuccess, errorMessage)
    {
        Data = data;
    }

    public static OperationResult<T> Success(T? data) => new(true, data, null);
    public new static OperationResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
}

public record OperationResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected OperationResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static OperationResult Success() => new(true, null);
    public static OperationResult Failure(string errorMessage) => new(false, errorMessage);
}