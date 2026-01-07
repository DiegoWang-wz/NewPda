using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

[Table("DX023FunctionalDetects")]
public class DX023FunctionalDetectsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long id { get; set; }

    [MaxLength(64)] 
    public string? palm_id { get; set; }

    [MaxLength(64)] 
    public string? inspector { get; set; }

    public string? remarks { get; set; }

    public bool? if_qualified { get; set; }

    [Column(TypeName = "datetime2(0)")] 
    public DateTime? calibrate_time { get; set; }
}