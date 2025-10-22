using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DexRobotPDA.DTOs;

public class FingerCalibrateDetectCreateDto : INotifyPropertyChanged
{
    private DateTime? _motor_using_time;
    private DateTime? _finger_using_time;
    private bool? _if_time_qualified;
    private string _finger_id = string.Empty;
    private string _test_action = string.Empty;
    private DateTime? _begin_time;
    private DateTime? _finish_time;

    private int? _consume_time;
    private double? _remote_angle;
    private double? _near_angle;
    private double? _motor_1_max;
    private double? _motor_2_max;
    private DateTime? _calibrate_time;
    private string _inspector = string.Empty;
    private string? _remarks;
    private bool? _if_qualified;
    private double? _electricity;
    private bool? _proximity_sensing;
    private bool? _normal_force;

    /// <summary>
    /// 电机使用时间（精确到小数点后2位）
    /// </summary>
    public DateTime? motor_using_time
    {
        get => _motor_using_time;
        set
        {
            if (_motor_using_time != value)
            {
                _motor_using_time = value;
                OnPropertyChanged();
                CalculateIfTimeQualified();
            }
        }
    }

    /// <summary>
    /// 手指使用时间（精确到小数点后2位）
    /// </summary>
    public DateTime? finger_using_time
    {
        get => _finger_using_time;
        set
        {
            if (_finger_using_time != value)
            {
                _finger_using_time = value;
                OnPropertyChanged();
                CalculateIfTimeQualified();
            }
        }
    }

    /// <summary>
    /// 时间是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_time_qualified
    {
        get => _if_time_qualified;
        set => SetProperty(ref _if_time_qualified, value);
    }

    /// <summary>
    /// 手指ID（设备标识，如 FING-001）
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string finger_id
    {
        get => _finger_id;
        set => SetProperty(ref _finger_id, value);
    }

    /// <summary>
    /// 测试动作（如 伸展测试、弯曲测试）
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string test_action
    {
        get => _test_action;
        set => SetProperty(ref _test_action, value);
    }

    /// <summary>
    /// 测试开始时间（精确到毫秒）
    /// </summary>
    public DateTime? begin_time
    {
        get => _begin_time;
        set
        {
            if (_begin_time != value)
            {
                _begin_time = value;
                OnPropertyChanged();
                OnTimePropertyChanged();
            }
        }
    }

    /// <summary>
    /// 测试结束时间（精确到毫秒）
    /// </summary>
    public DateTime? finish_time
    {
        get => _finish_time;
        set
        {
            if (_finish_time != value)
            {
                _finish_time = value;
                OnPropertyChanged();
                OnTimePropertyChanged();
            }
        }
    }

    /// <summary>
    /// 消耗时间（单位：毫秒）
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "耗时不能为负数")]
    public int? consume_time
    {
        get => _consume_time;
        set => SetProperty(ref _consume_time, value);
    }

    /// <summary>
    /// 仅用于显示的秒数（含毫秒），例如 1.234 s
    /// </summary>
    public double? consume_time_seconds => _consume_time.HasValue ? _consume_time.Value / 1000.0 : null;

    /// <summary>
    /// 远端角度（传感器读数）
    /// </summary>
    public double? remote_angle
    {
        get => _remote_angle;
        set => SetProperty(ref _remote_angle, value);
    }

    /// <summary>
    /// 近端角度（传感器读数）
    /// </summary>
    public double? near_angle
    {
        get => _near_angle;
        set => SetProperty(ref _near_angle, value);
    }

    /// <summary>
    /// 电机1最大参数（电机性能指标）
    /// </summary>
    public double? motor_1_max
    {
        get => _motor_1_max;
        set => SetProperty(ref _motor_1_max, value);
    }

    /// <summary>
    /// 电机2最大参数（电机性能指标）
    /// </summary>
    public double? motor_2_max
    {
        get => _motor_2_max;
        set => SetProperty(ref _motor_2_max, value);
    }

    /// <summary>
    /// 校准时间（精确到秒或毫秒，按业务需要）
    /// </summary>
    public DateTime? calibrate_time
    {
        get => _calibrate_time;
        set => SetProperty(ref _calibrate_time, value);
    }

    /// <summary>
    /// 检测人员（如 张工、李工）
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string inspector
    {
        get => _inspector;
        set => SetProperty(ref _inspector, value);
    }

    /// <summary>
    /// 备注（测试异常说明、正常记录等）
    /// </summary>
    public string? remarks
    {
        get => _remarks;
        set => SetProperty(ref _remarks, value);
    }

    /// <summary>
    /// 整体是否合格（0=不合格，1=合格）
    /// </summary>
    public bool? if_qualified
    {
        get => _if_qualified;
        set => SetProperty(ref _if_qualified, value);
    }

    /// <summary>
    /// 电量（设备剩余电量，如 98.2）
    /// </summary>
    public double? electricity
    {
        get => _electricity;
        set => SetProperty(ref _electricity, value);
    }

    /// <summary>
    /// 接近传感值（传感器读数）
    /// </summary>
    public bool? proximity_sensing
    {
        get => _proximity_sensing;
        set => SetProperty(ref _proximity_sensing, value);
    }

    /// <summary>
    /// 法向力（压力传感器读数）
    /// </summary>
    public bool? normal_force
    {
        get => _normal_force;
        set => SetProperty(ref _normal_force, value);
    }

    // INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// 当时间属性变化时调用
    /// </summary>
    private void OnTimePropertyChanged()
    {
        CalculateConsumeTime();
    }

    /// <summary>
    /// 计算耗时（finish_time - begin_time），结果为毫秒
    /// </summary>
    private void CalculateConsumeTime()
    {
        if (begin_time.HasValue && finish_time.HasValue)
        {
            TimeSpan timeDiff = finish_time.Value - begin_time.Value;
            if (timeDiff.Ticks >= 0)
            {
                // ★ 关键改动：毫秒为单位，四舍五入到最近 1ms
                consume_time = (int)Math.Round(timeDiff.TotalMilliseconds, MidpointRounding.AwayFromZero);
            }
            else
            {
                consume_time = null;
            }
        }
        else
        {
            consume_time = null;
        }
    }

    /// <summary>
    /// 计算时间是否合格
    /// 根据 finger_using_time 减去 motor_using_time 是否超过48小时
    /// </summary>
    private void CalculateIfTimeQualified()
    {
        if (finger_using_time.HasValue && motor_using_time.HasValue)
        {
            TimeSpan timeDiff = finger_using_time.Value - motor_using_time.Value;
            if_time_qualified = timeDiff.TotalHours >= 48;
        }
        else
        {
            if_time_qualified = null;
        }
    }
}
