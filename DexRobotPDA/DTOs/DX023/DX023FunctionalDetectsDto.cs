namespace DexRobotPDA.DTOs;

public class DX023FunctionalDetectsDto
{
    public long id { get; set; }

    public string? palm_id { get; set; }

    public string? inspector { get; set; }

    public string? remarks { get; set; }

    public bool? if_qualified { get; set; }

    public DateTime? calibrate_time { get; set; }
}