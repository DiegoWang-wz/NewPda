using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

public class MotorBindFingerDto
{
    public string motor_id { get; set; }
    public string finger_id { get; set; }
}