namespace DexRobotPDA.DTOs;

public class AddPalmDto
{
    public string palm_id { get; set; }

    public string task_id { get; set; }

    public string operator_id { get; set; }

    public string? remarks { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;

    public DateTime? updated_at { get; set; } = DateTime.Now;

    public bool is_qualified { get; set; } = false;
}