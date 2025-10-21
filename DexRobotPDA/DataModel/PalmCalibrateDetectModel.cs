using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

[Table("PalmCalibrateDetects")]
public class PalmCalibrateDetectModel
{
    /// <summary>
    /// 主键ID（自增）
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long id { get; set; }

    /// <summary>
    /// 手掌ID（设备标识，如 PALM-001）
    /// </summary>
    [MaxLength(64)]
    public string? palm_id { get; set; }

    /// <summary>
    /// 接近传感值（传感器读数）
    /// </summary>
    public bool? proximity_sensing { get; set; }

    /// <summary>
    /// 法向力（压力传感器读数）
    /// </summary>
    public double? normal_force { get; set; }

    /// <summary>
    /// 风扇功能状态（如 正常、异常）
    /// </summary>
    [MaxLength(64)]
    public string? fan_function { get; set; }

    /// <summary>
    /// 软件版本（如 V2.3.1）
    /// </summary>
    [MaxLength(64)]
    public string? software_version { get; set; }

    /// <summary>
    /// 电量（设备剩余电量）
    /// </summary>
    public double? electricity { get; set; }

    /// <summary>
    /// 检测人员
    /// </summary>
    [MaxLength(64)]
    public string? inspector { get; set; }

    /// <summary>
    /// 备注（功能异常说明、版本测试记录等）
    /// </summary>
    public string? remarks { get; set; }

    /// <summary>
    /// 整体是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_qualified { get; set; }
    
    [Column(TypeName = "datetime2(0)")]
    public DateTime? calibrate_time { get; set; }
    
    [MaxLength(64)]
    public string? hundred_times { get; set; }
}