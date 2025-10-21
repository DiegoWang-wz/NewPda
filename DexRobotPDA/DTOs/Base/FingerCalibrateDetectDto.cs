using System;
using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

/// <summary>
/// 手指校准检测完整DTO
/// 与数据库字段严格对应（小写+下划线命名），用于详情查询
/// </summary>
public class FingerCalibrateDetectDto
{
    /// <summary>
    /// 主键ID（自增）
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 电机使用时间（精确到小数点后2位）
    /// </summary>
    public DateTime? motor_using_time { get; set; }

    /// <summary>
    /// 手指使用时间（精确到小数点后2位）
    /// </summary>
    public DateTime? finger_using_time { get; set; }

    /// <summary>
    /// 时间是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_time_qualified { get; set; }

    /// <summary>
    /// 手指ID（设备标识，如 FING-001）
    /// </summary>
    [MaxLength(64)]
    public string? finger_id { get; set; }

    /// <summary>
    /// 测试动作（如 伸展测试、弯曲测试）
    /// </summary>
    [MaxLength(64)]
    public string? test_action { get; set; }

    /// <summary>
    /// 测试开始时间（精确到小数点后3位）
    /// </summary>
    public DateTime? begin_time { get; set; }

    /// <summary>
    /// 测试结束时间（精确到小数点后3位）
    /// </summary>
    public DateTime? finish_time { get; set; }

    /// <summary>
    /// 消耗时间（单位：秒）
    /// </summary>
    public int? consume_time { get; set; }

    /// <summary>
    /// 远端角度（传感器读数）
    /// </summary>
    public double? remote_angle { get; set; }

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
    public DateTime? calibrate_time { get; set; }

    /// <summary>
    /// 检测人员（如 张工、李工）
    /// </summary>
    [MaxLength(64)]
    public string? inspector { get; set; }

    /// <summary>
    /// 备注（测试异常说明、正常记录等）
    /// </summary>
    public string? remarks { get; set; }

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
    public bool? proximity_sensing { get; set; }

    /// <summary>
    /// 法向力（压力传感器读数）
    /// </summary>
    public double? normal_force { get; set; }
}
    