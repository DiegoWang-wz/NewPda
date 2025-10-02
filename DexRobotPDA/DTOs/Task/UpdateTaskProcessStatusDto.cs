namespace DexRobotPDA.DTOs;

public class UpdateTaskProcessStatusDto
{
    public string task_id { get; set; }
    public string process { get; set; }
    public byte status { get; set; }
}