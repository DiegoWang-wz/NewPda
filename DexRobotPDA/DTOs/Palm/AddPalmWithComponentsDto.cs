namespace DexRobotPDA.DTOs;

// AddPalmWithComponentsDto.cs
public class AddPalmWithComponentsDto
{
    public string palm_id { get; set; }
    public string task_id { get; set; }
    public string operator_id { get; set; }
    public string remarks { get; set; }
    public List<string> component_ids { get; set; } = new List<string>();
}
