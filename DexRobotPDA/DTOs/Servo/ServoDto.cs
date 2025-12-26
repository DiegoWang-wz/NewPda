namespace DexRobotPDA.DTOs;

public class ServoDto
{
    public long id { get; set; }

    public string servo_id { get; set; }

    public string task_id { get; set; }

    public string operator_id { get; set; }

    public string? remarks { get; set; }

    public string superior_id { get; set; }
    public DateTime created_at { get; set; } = DateTime.Now;

    public DateTime? updated_at { get; set; } = DateTime.Now;
    public bool is_qualified { get; set; } = false;

    public int type { get; set; }
}