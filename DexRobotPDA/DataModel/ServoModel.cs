using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 成品舵机表（记录舵机生产详情及物料关联）
    /// </summary>
    [Table("Servos")]
    public class ServoModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 舵机ID（字母数字编码，如MOT-202409-001）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Servo_id { get; set; }

        /// <summary>
        /// 生产单号
        /// </summary>
        [MaxLength(64)]
        public string? task_id { get; set; }
        /// <summary>
        /// 操作员ID（关联员工表，确保操作可追溯）
        /// </summary>
        [MaxLength(64)]
        public string operator_id { get; set; }

        /// <summary>
        /// 实际操作时间
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }

        /// <summary>
        /// 是否合格：0=不合格(NG)，1=合格(OK)
        /// </summary>
        public bool is_qualified { get; set; } = false;

        /// <summary>
        /// 备注（如不合格原因、特殊说明）
        /// </summary>
        public string? remarks { get; set; }

        /// <summary>
        /// 绑定成品手指ID（允许为空，如未绑定手指）
        /// </summary>
        [MaxLength(64)]
        public string? superior_id { get; set; }
        
        [Required]
        public int type { get; set; }
        
        
    }
}