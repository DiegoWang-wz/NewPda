namespace DexRobotPDA.DTOs;

public class AddServoDto
{

    public string servo_id { get; set; }

    public string task_id { get; set; }

    public string operator_id { get; set; }

    public string? superior_id { get; set; }
    public string? remarks { get; set; }

    public DateTime? created_at { get; set; } = DateTime.Now;
    
    public bool is_qualified { get; set; } = true;
    
    public int type { get; set; }
}