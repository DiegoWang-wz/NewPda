namespace DexRobotPDA.DTOs;

public class MotorDataDto
{
    public MotorDto motor { get; set; }
    public List<MotorWormDetectDto> detects { get; set; }
}