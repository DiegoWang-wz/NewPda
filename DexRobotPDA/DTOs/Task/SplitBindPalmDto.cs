using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

public class SplitBindPalmDto
{
    [Required]
    public string split_id { get; set; }

    /// <summary>
    /// 手掌ID
    /// </summary>
    [Required]
    public string palm_id { get; set; }
}