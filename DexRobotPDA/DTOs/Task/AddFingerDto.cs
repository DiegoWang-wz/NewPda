namespace DexRobotPDA.DTOs;

public class AddFingerDto
{
    public string finger_id { get; set; }

    public string task_id { get; set; }

    public string operator_id { get; set; }

    public string? remarks { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;
    public bool is_qualified { get; set; } = false;
    public bool is_thumb { get; set; } = false;
}