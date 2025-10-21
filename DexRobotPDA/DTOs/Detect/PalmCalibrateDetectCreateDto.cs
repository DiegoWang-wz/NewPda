namespace DexRobotPDA.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// 手掌校准检测创建DTO
/// 用于新增记录，包含必要的必填字段
/// </summary>
public class PalmCalibrateDetectCreateDto
{
    /// <summary>
    /// 手掌ID（设备标识）
    /// </summary>
    [Required(ErrorMessage = "手掌ID不能为空")]
    [MaxLength(64, ErrorMessage = "手掌ID长度不能超过64个字符")]
    public string palm_id { get; set; } = string.Empty;

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
    [MaxLength(64, ErrorMessage = "风扇功能状态描述不能超过64个字符")]
    public string? fan_function { get; set; }

    /// <summary>
    /// 软件版本（如 V2.3.1）
    /// </summary>
    [Required(ErrorMessage = "软件版本不能为空")]
    [MaxLength(64, ErrorMessage = "软件版本长度不能超过64个字符")]
    public string software_version { get; set; } = string.Empty;

    /// <summary>
    /// 电量（设备剩余电量）
    /// </summary>
    public double? electricity { get; set; }

    /// <summary>
    /// 检测人员
    /// </summary>
    [Required(ErrorMessage = "检测人员不能为空")]
    [MaxLength(64, ErrorMessage = "检测人员姓名不能超过64个字符")]
    public string inspector { get; set; } = string.Empty;

    /// <summary>
    /// 备注（功能异常说明、版本测试记录等）
    /// </summary>
    public string? remarks { get; set; }

    /// <summary>
    /// 整体是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_qualified { get; set; }
    
    public DateTime? calibrate_time { get; set; }
    
    public string? hundred_times { get; set; }
}
