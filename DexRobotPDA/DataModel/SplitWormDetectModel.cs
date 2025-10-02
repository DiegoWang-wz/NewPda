using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

/// <summary>
/// 分割蜗杆检测记录表实体
/// 字段名严格对应数据库表 SplitWormDetects 的小写_格式
/// </summary>
[Table("SplitWormDetects")] // 映射数据库表名（保持原表名大小写）
public class SplitWormDetectModel
{
    /// <summary>
    /// 主键ID（自增）
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long id { get; set; } // 字段名：id（与数据库一致）

    /// <summary>
    /// 分割编号（关联分割任务/产品的唯一标识）
    /// </summary>
    [MaxLength(64, ErrorMessage = "分割编号长度不能超过64个字符")]
    public string? split_id { get; set; } // 字段名：split_id（与数据库一致）

    /// <summary>
    /// 粘结时间
    /// </summary>
    public DateTime? combine_time { get; set; } // 字段名：combine_time（与数据库一致）

    /// <summary>
    /// 投入使用时间
    /// </summary>
    public DateTime? using_time { get; set; } // 字段名：using_time（与数据库一致）

    /// <summary>
    /// 检测人员（姓名/工号）
    /// </summary>
    [MaxLength(64, ErrorMessage = "检测人员名称长度不能超过64个字符")]
    public string? inspector { get; set; } // 字段名：inspector（与数据库一致）

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? remarks { get; set; } // 字段名：remarks（与数据库一致）

    /// <summary>
    /// 是否合格（0=不合格，1=合格）
    /// </summary>
    public bool if_qualified { get; set; } // 字段名：if_qualified（与数据库一致）
    
    /// <summary>
    /// 分割编号（关联分割任务/产品的唯一标识）
    /// </summary>
    [MaxLength(64, ErrorMessage = "分割编号长度不能超过64个字符")]
    public string? motor_id { get; set; } // 字段名：split_id（与数据库一致）
}