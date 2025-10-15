namespace DexRobotPDA.DTOs;

public class EmployeeDto
{
    public long id { get; set; }
    
    public string employee_id { get; set; }
    
    public string employee_name { get; set; }
    
    public byte gender { get; set; }
    
    public DateTime birthday { get; set; }
    
    public string phone { get; set; }
    
    public string department { get; set; }
    
    public string team_id { get; set; }
    
    public string position { get; set; }
    
    public byte status { get; set; } = 1;
    
    // public string password { get; set; }
}