using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    [Table("Splits")]
    public class SplitModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 成品手指ID（支持Unicode，如FG-202409-001）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string split_id { get; set; }

        /// <summary>
        /// 关联生产单号
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string task_id { get; set; }

        /// <summary>
        /// 操作员ID（关联员工表，确保可追溯）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string operator_id { get; set; }
        
        /// <summary>
        /// 备注（如组装异常、特殊要求）
        /// </summary>
        public string? remarks { get; set; }

        /// <summary>
        /// 绑定成品手掌ID（允许为空，如未绑定手掌）
        /// </summary>
        [MaxLength(64)]
        public string? palm_id { get; set; }

        /// <summary>
        /// 手指创建时间
        /// </summary>
        [Required]
        public DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// 手指更新时间（自动更新）
        /// </summary>
        [Required]
        public DateTime? updated_at { get; set; } = DateTime.Now;
        
        [Required]
        public bool is_qualified { get; set; } = false;

        // 导航属性
        public ProductTaskModel TaskModel { get; set; }
        public MaterialModel SplitMaterial { get; set; }
        public EmployeeModel Operator { get; set; }
        public PalmModel Palm { get; set; }
    }
}