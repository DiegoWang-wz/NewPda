namespace DexRobotPDA.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// 手掌校准检测更新DTO
/// 用于更新记录，所有字段均为可选
/// </summary>
public class PalmCalibrateDetectUpdateDto
{
    /// <summary>
    /// 手掌ID（设备标识）
    /// </summary>
    [MaxLength(64, ErrorMessage = "手掌ID长度不能超过64个字符")]
    public string? palm_id { get; set; }

    /// <summary>
    /// 接近传感值（传感器读数）
    /// </summary>
    public double? proximity_sensing { get; set; }

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
    [MaxLength(64, ErrorMessage = "软件版本长度不能超过64个字符")]
    public string? software_version { get; set; }

    /// <summary>
    /// 电量（设备剩余电量）
    /// </summary>
    [Range(0, 100, ErrorMessage = "电量值必须在0-100之间")]
    public double? electricity { get; set; }

    /// <summary>
    /// 检测人员
    /// </summary>
    [MaxLength(64, ErrorMessage = "检测人员姓名不能超过64个字符")]
    public string? inspector { get; set; }

    /// <summary>
    /// 备注（功能异常说明、版本测试记录等）
    /// </summary>
    public string? remarks { get; set; }

    /// <summary>
    /// 整体是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_qualified { get; set; }
    
    public DateTime? calibrate_time { get; set; }
}
