using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DexRobotPDA.DataModel;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 成品手掌表（记录手掌组装详情）
    /// </summary>
    [Table("Palms")]
    public class PalmModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 成品手掌ID（支持Unicode，如PM-202409-001）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string palm_id { get; set; }

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
        /// 备注（如组装异常、质检说明）
        /// </summary>
        public string? remarks { get; set; }

        /// <summary>
        /// 手掌创建时间
        /// </summary>
        [Required]
        public DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// 手掌更新时间（自动更新）
        /// </summary>
        public DateTime? updated_at { get; set; } = DateTime.Now;
        
        [Required]
        public bool is_qualified { get; set; } = false;
        
    }
}