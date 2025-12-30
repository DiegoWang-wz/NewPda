namespace DexRobotPDA.DTOs;


public class FingerDataDto
{
    public FingerDto finger { get; set; }
    public List<MotorDataDto> motors { get; set; }
    public List<ServoDataDto> servos { get; set; }
    public List<FingerCalibrateDetectDto> detect4 { get; set; }
    public List<SplitCalibrateDetectDto> detect3 { get; set; }
    public List<SplitWormDetectDto> detect2 { get; set; }
}