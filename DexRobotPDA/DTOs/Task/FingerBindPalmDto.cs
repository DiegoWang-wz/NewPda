using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

public class FingerBindPalmDto
{
    public string finger_id { get; set; }
    public string palm_id { get; set; }
}