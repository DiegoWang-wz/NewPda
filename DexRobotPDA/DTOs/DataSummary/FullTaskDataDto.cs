namespace DexRobotPDA.DTOs;

public class FullTaskDataDto
{
    public ProductTaskDto task { get; set; } = default!;
    public PalmDto palm { get; set; } = default!;
    public ServoDto? rotateServo { get; set; }   // 旋转舵机可能没有
    public List<FingerDataDto> fingers { get; set; } = new();

    /// <summary>
    /// 兼容老逻辑：DX021 的 palm 检测（原 Detect5 映射出来的）
    /// DX023 情况下通常为空（除非你也决定给 DX023 填充 Detect5）
    /// </summary>
    public List<PalmCalibrateDetectDto> detects { get; set; } = new();

    /// <summary>
    /// ✅ DX023：校准检测（按 palm_id）
    /// </summary>
    public List<DX023CalibrateDetectsDto> dx023CalibrateDetects { get; set; } = new();

    /// <summary>
    /// ✅ DX023：功能检测（按 palm_id）
    /// </summary>
    public List<DX023FunctionalDetectsDto> dx023FunctionalDetects { get; set; } = new();
}
