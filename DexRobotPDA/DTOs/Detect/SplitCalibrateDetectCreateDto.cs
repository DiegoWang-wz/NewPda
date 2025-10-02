using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DexRobotPDA.DTOs;

/// <summary>
/// 分割校准检测DTO
/// 字段名严格采用小写_格式，与Model、数据库完全对齐
/// </summary>
public class SplitCalibrateDetectCreateDto : INotifyPropertyChanged
{
    private string? _split_id;
    private double? _electricity;
    private string? _test_action;
    private DateTime? _begin_time;
    private DateTime? _finish_time;
    private int? _consume_time;
    private double? _remote_angle;
    private double? _near_angle;
    private double? _motor_1_max;
    private double? _motor_2_max;
    private DateTime? _calibrate_time;
    private string? _inspector;
    private string? _remarks;
    private bool? _if_qualified;

    /// <summary>
    /// 分割编号（关联分割任务/产品的唯一标识，必填）
    /// </summary>
    [Required(ErrorMessage = "分割编号不能为空")]
    [MaxLength(64, ErrorMessage = "分割编号长度不能超过64个字符")]
    public string? split_id
    {
        get => _split_id;
        set => SetProperty(ref _split_id, value);
    }

    /// <summary>
    /// 电气参数（如电压/电流）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "电气参数不能为负数")]
    public double? electricity
    {
        get => _electricity;
        set => SetProperty(ref _electricity, value);
    }

    /// <summary>
    /// 测试动作类型（如“正向校准”“反向校准”）
    /// </summary>
    [MaxLength(64, ErrorMessage = "测试动作类型长度不能超过64个字符")]
    public string? test_action
    {
        get => _test_action;
        set => SetProperty(ref _test_action, value);
    }

    /// <summary>
    /// 测试开始时间
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
    /// 测试完成时间
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
    /// 耗时（单位：秒/毫秒，需与业务统一）
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "耗时不能为负数")]
    public int? consume_time
    {
        get => _consume_time;
        set => SetProperty(ref _consume_time, value);
    }

    /// <summary>
    /// 远端角度
    /// </summary>
    public double? remote_angle
    {
        get => _remote_angle;
        set => SetProperty(ref _remote_angle, value);
    }

    /// <summary>
    /// 近端角度
    /// </summary>
    public double? near_angle
    {
        get => _near_angle;
        set => SetProperty(ref _near_angle, value);
    }

    /// <summary>
    /// 电机1最大值（负载/转速等参数）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "电机1最大值不能为负数")]
    public double? motor_1_max
    {
        get => _motor_1_max;
        set => SetProperty(ref _motor_1_max, value);
    }

    /// <summary>
    /// 电机2最大值（负载/转速等参数）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "电机2最大值不能为负数")]
    public double? motor_2_max
    {
        get => _motor_2_max;
        set => SetProperty(ref _motor_2_max, value);
    }

    /// <summary>
    /// 校准时间（最终校准完成时间）
    /// </summary>
    public DateTime? calibrate_time
    {
        get => _calibrate_time;
        set => SetProperty(ref _calibrate_time, value);
    }

    /// <summary>
    /// 检测人员（姓名/工号）
    /// </summary>
    [MaxLength(64, ErrorMessage = "检测人员名称长度不能超过64个字符")]
    public string? inspector
    {
        get => _inspector;
        set => SetProperty(ref _inspector, value);
    }

    /// <summary>
    /// 备注信息（如校准异常说明）
    /// </summary>
    public string? remarks
    {
        get => _remarks;
        set => SetProperty(ref _remarks, value);
    }

    /// <summary>
    /// 是否合格（0=不合格，1=合格，必填）
    /// </summary>
    [Required(ErrorMessage = "请选择合格状态")]
    public bool? if_qualified
    {
        get => _if_qualified;
        set => SetProperty(ref _if_qualified, value);
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
        // 自动计算耗时
        CalculateConsumeTime();
    }

    /// <summary>
    /// 计算耗时（动作结束时间 - 发起时间）
    /// </summary>
    public void CalculateConsumeTime()
    {
        if (begin_time.HasValue && finish_time.HasValue)
        {
            TimeSpan timeDiff = finish_time.Value - begin_time.Value;
            if (timeDiff.TotalMilliseconds >= 0)
            {
                double consumeTime = Math.Round(timeDiff.TotalSeconds, 3);
                consume_time = (int)consumeTime;
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
}
