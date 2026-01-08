namespace DexRobotPDA.DTOs;

public class AddDX023FunctionalDetectsDto
{
    public string? palm_id { get; set; }

    public string? inspector { get; set; }

    public string? remarks { get; set; }

    public bool if_qualified { get; set; } = false;

    public DateTime? calibrate_time { get; set; }
}