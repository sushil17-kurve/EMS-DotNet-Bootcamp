namespace EMS.Application.DTOs.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponseDto<T> Ok(T data, string message = "")
        => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponseDto<T> Fail(string error)
        => new()
        {
            Success = false,
            Errors = new List<string> { error }
        };
}