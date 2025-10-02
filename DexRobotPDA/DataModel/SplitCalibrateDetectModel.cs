using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

/// <summary>
/// 分割校准检测记录表实体
/// 字段名严格对应数据库表 SplitCalibrateDetects 的小写_格式
/// </summary>
[Table("SplitCalibrateDetects")] // 映射数据库表名（保持原表名大小写）
public class SplitCalibrateDetectModel
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
    /// 电气参数（如电压/电流）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "电气参数不能为负数")]
    public double? electricity { get; set; } // 字段名：electricity（与数据库一致）

    /// <summary>
    /// 测试动作类型（如“正向校准”“反向校准”）
    /// </summary>
    [MaxLength(64, ErrorMessage = "测试动作类型长度不能超过64个字符")]
    public string? test_action { get; set; } // 字段名：test_action（与数据库一致）

    /// <summary>
    /// 测试开始时间
    /// </summary>
    public DateTime? begin_time { get; set; } // 字段名：begin_time（与数据库一致）

    /// <summary>
    /// 测试完成时间
    /// </summary>
    public DateTime? finish_time { get; set; } // 字段名：finish_time（与数据库一致）

    /// <summary>
    /// 耗时（单位：秒/毫秒，需与业务统一）
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "耗时不能为负数")]
    public int? consume_time { get; set; } // 字段名：consume_time（与数据库一致）

    /// <summary>
    /// 远端角度
    /// </summary>
    public double? remote_angle { get; set; } // 字段名：remote_angle（与数据库一致）

    /// <summary>
    /// 近端角度
    /// </summary>
    public double? near_angle { get; set; } // 字段名：near_angle（与数据库一致）

    /// <summary>
    /// 电机1最大值（负载/转速等参数）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "电机1最大值不能为负数")]
    public double? motor_1_max { get; set; } // 字段名：motor_1_max（与数据库一致）

    /// <summary>
    /// 电机2最大值（负载/转速等参数）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "电机2最大值不能为负数")]
    public double? motor_2_max { get; set; } // 字段名：motor_2_max（与数据库一致）

    /// <summary>
    /// 校准时间（最终校准完成时间）
    /// </summary>
    public DateTime? calibrate_time { get; set; } // 字段名：calibrate_time（与数据库一致）

    /// <summary>
    /// 检测人员（姓名/工号）
    /// </summary>
    [MaxLength(64, ErrorMessage = "检测人员名称长度不能超过64个字符")]
    public string? inspector { get; set; } // 字段名：inspector（与数据库一致）

    /// <summary>
    /// 备注信息（如校准异常说明）
    /// </summary>
    public string? remarks { get; set; } // 字段名：remarks（与数据库一致）

    /// <summary>
    /// 是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_qualified { get; set; } // 字段名：if_qualified（与数据库一致）
}