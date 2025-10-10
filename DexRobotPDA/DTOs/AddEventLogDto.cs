namespace DexRobotPDA.DTOs;

public class AddEventLogDto
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public string event_type { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    public string operator_id { get; set; }  // 使用@转义关键字

    /// <summary>
    /// 具体明细
    /// </summary>
    public string event_detail { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime operate_time { get; set; }
    
    public bool is_qualified { get; set; } = false;
}
