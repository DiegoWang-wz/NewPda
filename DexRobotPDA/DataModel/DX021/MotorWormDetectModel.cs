using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

[Table("MotorWormDetects")]
public class MotorWormDetectModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long id { get; set; }

    public string? motor_id { get; set; }

    public double ? distance_before { get; set; }

    public double ? force { get; set; }

    public double ? distance_after { get; set; }

    public double ? distance_result { get; set; }

    public DateTime? combine_time { get; set; }

    public DateTime? using_time { get; set; }

    public string? inspector_id { get; set; }
    
    public string? remarks { get; set; }
    
    public bool if_qualified { get; set; }
}