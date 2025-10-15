namespace DexRobotPDA.DTOs;

public class FingerDto
{
    public long id { get; set; }

    public string finger_id { get; set; }

    public string task_id { get; set; }

    public string operator_id { get; set; }

    public string? remarks { get; set; }

    public string palm_id { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;

    public DateTime? updated_at { get; set; } = DateTime.Now;
    
    public bool is_qualified { get; set; } = false;
    
    public int type { get; set; }
    
}