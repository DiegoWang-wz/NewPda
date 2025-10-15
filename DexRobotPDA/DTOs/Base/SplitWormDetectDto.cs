using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

/// <summary>
/// 分割蜗杆检测DTO
/// 字段名严格采用小写_格式，与Model、数据库完全对齐
/// </summary>
public class SplitWormDetectDto
{
    /// <summary>
    /// 主键ID（查询/更新时使用，创建时无需传递）
    /// </summary>
    public long id { get; set; } // 字段名：id（与Model一致）

    /// <summary>
    /// 分割编号（关联分割任务/产品的唯一标识，必填）
    /// </summary>
    [Required(ErrorMessage = "分割编号不能为空")]
    [MaxLength(64, ErrorMessage = "分割编号长度不能超过64个字符")]
    public string? finger_id { get; set; } // 字段名：finger_id（与Model一致）

    /// <summary>
    /// 粘结时间
    /// </summary>
    public DateTime? combine_time { get; set; } // 字段名：combine_time（与Model一致）

    /// <summary>
    /// 投入使用时间
    /// </summary>
    public DateTime? using_time { get; set; } // 字段名：using_time（与Model一致）

    /// <summary>
    /// 检测人员（姓名/工号）
    /// </summary>
    [MaxLength(64, ErrorMessage = "检测人员名称长度不能超过64个字符")]
    public string? inspector { get; set; } // 字段名：inspector（与Model一致）

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? remarks { get; set; } // 字段名：remarks（与Model一致）

    /// <summary>
    /// 是否合格（0=不合格，1=合格，必填）
    /// </summary>
    [Required(ErrorMessage = "请选择合格状态")]
    public bool if_qualified { get; set; } // 字段名：if_qualified（与Model一致）
    
    public string? motor_id { get; set; } // 字段名：finger_id（与数据库一致
}