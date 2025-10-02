namespace DexRobotPDA.DTOs;

public class AddMotorDto
{
    public string motor_id { get; set; }

    public string task_id { get; set; }

    public string worm_material_id { get; set; } = "MAT-001";

    public string adhesive_material_id { get; set; } = "MAT-002";

    public string operator_id { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;

    public bool is_qualified { get; set; } = false;

    public string? remarks { get; set; }
}