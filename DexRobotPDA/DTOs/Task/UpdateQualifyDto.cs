using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

public class UpdateQualifyDto
{
    [Required] public string id { get; set; }
    [Required] public bool qualified { get; set; }
}