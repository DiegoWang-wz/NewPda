using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel;

[Table("EventLog")]
public class EventLogModel
{
    /// <summary>
    /// 序号（自增主键）
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [Required]
    public string event_type { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    [Required]
    public string operator_id { get; set; } 

    /// <summary>
    /// 具体明细
    /// </summary>
    [Required]
    public string event_detail { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [Required]
    public DateTime operate_time { get; set; } = DateTime.Now;
    
    [Required]
    public bool is_qualified { get; set; } = false;
}
