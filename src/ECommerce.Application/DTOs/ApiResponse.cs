namespace ECommerce.Application.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = error
        };
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }

    public static ApiResponse SuccessResult(string message = "Operation completed successfully")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse ErrorResult(string message, string error)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Error = error
        };
    }
}
