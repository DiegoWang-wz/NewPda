namespace DexRobotPDA.DTOs;

public class FullTaskDataDto
{
    public ProductTaskDto task { get; set; }
    public PalmDto palm { get; set; }
    public List<FingerDataDto> fingers { get; set; }
    public List<PalmCalibrateDetectDto> detects { get; set; }
}