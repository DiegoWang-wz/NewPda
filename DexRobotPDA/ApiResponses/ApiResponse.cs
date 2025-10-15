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
    public string Msg { get; set; }
    
    /// <summary>
    /// 数据
    /// </summary>
    public object ResultData { get; set; }
}

public class ApiResponse<T>
{
    public int ResultCode { get; set; }
    public string Msg { get; set; } = string.Empty;
    public T? ResultData { get; set; }
}