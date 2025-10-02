namespace DexRobotPDA.DTOs;

public class MotorDto
{

    public long id { get; set; }

    public string motor_id { get; set; }

    public string task_id { get; set; }

    public string worm_material_id { get; set; }

    public string adhesive_material_id { get; set; }

    public string operator_id { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;

    public bool is_qualified { get; set; } = false;

    public string? remarks { get; set; }

    public string? finger_id { get; set; }
    
    public DateTime? update_at { get; set; } = DateTime.Now;
    
}