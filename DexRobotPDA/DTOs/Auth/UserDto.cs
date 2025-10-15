namespace DexRobotPDA.DTOs;

public class UserDto
{
    public string employee_id { get; set; }
    public string employee_name { get; set; }
    
    public byte gender { get; set; }
    
    public string phone { get; set; }
    public string department { get; set; }
    
    public string team_id { get; set; }
    
    public string position { get; set; }
}