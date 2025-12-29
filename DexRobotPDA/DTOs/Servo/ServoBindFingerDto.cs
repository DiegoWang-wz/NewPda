namespace DexRobotPDA.DTOs;

public class ServoBindFingerDto
{
    public string finger_id { get; set; }
    public string task_id { get; set; }
    public string operator_id { get; set; }
    public string servo_id1 { get; set; }
    public string? servo_id2 { get; set; }
    public string? remarks { get; set; }
}