namespace DexRobotPDA.DTOs;

public class MotorWormDetectDto
{
    public long id { get; set; }

    public string? motor_id { get; set; }

    public double? distance_before { get; set; }

    public double? force { get; set; }

    public double? distance_after { get; set; }

    public double? distance_result { get; set; }

    public DateTime? combine_time { get; set; }

    public DateTime? using_time { get; set; }

    public string? inspector_id { get; set; }
    
    public string? remarks { get; set; }
    
    public bool if_qualified { get; set; }
}