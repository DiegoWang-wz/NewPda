namespace DexRobotPDA.DTOs;

public class AddFingerWithMotorsDto
{
    public string finger_id { get; set; }
    public string task_id { get; set; }
    public string operator_id { get; set; }
    public string remarks { get; set; }
    public bool is_thumb { get; set; }
    public List<string> motor_ids { get; set; } = new List<string>();
}