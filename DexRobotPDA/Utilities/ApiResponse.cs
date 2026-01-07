namespace DexRobotPDA.ApiResponses;

public class ApiResponse
{
    /// <summary>
    /// 结果编码
    /// </summary>
    public int ResultCode { get; set; }

    /// <summary>
    /// 结果信息
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 数据
    /// </summary>
    public object? ResultData { get; set; }
}

public class ApiResponse<T>
{
    public int ResultCode { get; set; }
    public string Msg { get; set; } = string.Empty;
    public T? ResultData { get; set; }

    public static ApiResponse<T> Ok(T? data, string msg = "OK")
        => new ApiResponse<T> { ResultCode = 1, Msg = msg, ResultData = data };

    public static ApiResponse<T> Fail(string msg, int code = 0, T? data = default)
        => new ApiResponse<T> { ResultCode = code, Msg = msg, ResultData = data };

    public static ApiResponse<T> Canceled(string msg = "Request canceled")
        => new ApiResponse<T> { ResultCode = -2, Msg = msg, ResultData = default };

    public static ApiResponse<T> NotFound(string msg = "Not Found")
        => new ApiResponse<T> { ResultCode = 404, Msg = msg, ResultData = default };

    // 可选：参数错误专用
    public static ApiResponse<T> BadRequest(string msg = "Bad Request")
        => new ApiResponse<T> { ResultCode = 0, Msg = msg, ResultData = default };
}