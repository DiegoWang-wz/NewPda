namespace DexRobotPDA.DTOs;

public class AddMotorDto
{
    public string motor_id { get; set; }

    public string? task_id { get; set; }
    
    public string operator_id { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;

    public bool is_qualified { get; set; } = false;

    public string? remarks { get; set; }
}