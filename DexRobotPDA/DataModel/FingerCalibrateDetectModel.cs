using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

[Table("FingerCalibrateDetects")]
public class FingerCalibrateDetectModel
{
    /// <summary>
    /// 主键ID（自增）
    /// </summary>
    [Key] // 标记为主键
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自增特性（对应 SQL 的 IDENTITY(1,1)）
    public long id { get; set; }

    /// <summary>
    /// 电机使用时间（精确到小数点后2位）
    /// </summary>
    [Column(TypeName = "datetime2(2)")] // 匹配 SQL 的 datetime2(2) 类型
    public DateTime? motor_using_time { get; set; } // 允许 null（若表中允许空值）

    /// <summary>
    /// 手指使用时间（精确到小数点后2位）
    /// </summary>
    [Column(TypeName = "datetime2(2)")]
    public DateTime? finger_using_time { get; set; }

    /// <summary>
    /// 时间是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_time_qualified { get; set; } // BIT 类型映射为 bool?（允许 null）

    /// <summary>
    /// 手指ID（设备标识，如 FING-001）
    /// </summary>
    [MaxLength(64)] // 匹配 SQL 的 NVARCHAR(64) 长度限制
    public string? finger_id { get; set; }

    /// <summary>
    /// 测试动作（如 伸展测试、弯曲测试）
    /// </summary>
    [MaxLength(64)]
    public string? test_action { get; set; }

    /// <summary>
    /// 测试开始时间（精确到小数点后3位）
    /// </summary>
    [Column(TypeName = "datetime2(3)")]
    public DateTime? begin_time { get; set; }

    /// <summary>
    /// 测试结束时间（精确到小数点后3位）
    /// </summary>
    [Column(TypeName = "datetime2(3)")]
    public DateTime? finish_time { get; set; }

    /// <summary>
    /// 消耗时间（单位：秒）
    /// </summary>
    public int? consume_time { get; set; }

    /// <summary>
    /// 远端角度（传感器读数）
    /// </summary>
    public double? remote_angle { get; set; } // float 类型映射为 double（EF Core 推荐）

    /// <summary>
    /// 近端角度（传感器读数）
    /// </summary>
    public double? near_angle { get; set; }

    /// <summary>
    /// 电机1最大参数（电机性能指标）
    /// </summary>
    public double? motor_1_max { get; set; }

    /// <summary>
    /// 电机2最大参数（电机性能指标）
    /// </summary>
    public double? motor_2_max { get; set; }

    /// <summary>
    /// 校准时间（精确到秒）
    /// </summary>
    [Column(TypeName = "datetime2(0)")]
    public DateTime? calibrate_time { get; set; }

    /// <summary>
    /// 检测人员（如 张工、李工）
    /// </summary>
    [MaxLength(64)]
    public string? inspector { get; set; }

    /// <summary>
    /// 备注（测试异常说明、正常记录等）
    /// </summary>
    public string? remarks { get; set; } // NVARCHAR(MAX) 映射为 string

    /// <summary>
    /// 整体是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_qualified { get; set; }

    /// <summary>
    /// 电量（设备剩余电量，如 98.2）
    /// </summary>
    public double? electricity { get; set; }

    /// <summary>
    /// 接近传感值（传感器读数）
    /// </summary>
    public double? proximity_sensing { get; set; }

    /// <summary>
    /// 法向力（压力传感器读数）
    /// </summary>
    public double? normal_force { get; set; }
    
}