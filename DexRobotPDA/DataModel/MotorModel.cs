using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 成品电机表（记录电机生产详情及物料关联）
    /// </summary>
    [Table("Motors")]
    public class MotorModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 电机ID（字母数字编码，如MOT-202409-001）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string motor_id { get; set; }

        /// <summary>
        /// 生产单号
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? task_id { get; set; }
        /// <summary>
        /// 操作员ID（关联员工表，确保操作可追溯）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string operator_id { get; set; }

        /// <summary>
        /// 实际操作时间
        /// </summary>
        [Required]
        public DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否合格：0=不合格(NG)，1=合格(OK)
        /// </summary>
        [Required]
        public bool is_qualified { get; set; } = false;

        /// <summary>
        /// 备注（如不合格原因、特殊说明）
        /// </summary>
        public string? remarks { get; set; }

        /// <summary>
        /// 绑定成品手指ID（允许为空，如未绑定手指）
        /// </summary>
        [MaxLength(64)]
        public string? finger_id { get; set; }
        
        public DateTime? updated_at { get; set; }
        
    }
}