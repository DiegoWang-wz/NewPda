using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DexRobotPDA.DataModel;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 生产批次表（一个生产任务可拆分为多个批次）
    /// </summary>
    [Table("Production_Batches")]
    public class ProductionBatchModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 关联生产任务ID（与Product_Tasks.task_id类型一致）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string task_id { get; set; }

        /// <summary>
        /// 批次号（系统生成规则：PO-日期-序号，如PO-20240909-001）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string batch_number { get; set; }

        /// <summary>
        /// 实际生产数量（允许为空，如未生产完成时）
        /// </summary>
        public int? quantity_produced { get; set; }

        /// <summary>
        /// 生产日期（仅日期，便于按日统计）
        /// </summary>
        [Required]
        public DateTime production_date { get; set; }

        /// <summary>
        /// 关联生产班组ID（允许为空，如未分配班组）
        /// </summary>
        [MaxLength(64)]
        public string team_id { get; set; }

        /// <summary>
        /// 使用设备ID（需后续补充设备表关联，当前预留）
        /// </summary>
        public long? equipment_id { get; set; }

        /// <summary>
        /// 主要原材料批次（支持多批次，如M20240901-001,M20240901-002）
        /// </summary>
        [MaxLength(500)]
        public string raw_material_batch { get; set; }

        /// <summary>
        /// 质量状态：0=待检，1=合格，2=不合格（允许为空，如未质检）
        /// </summary>
        public byte? quality_status { get; set; }

        /// <summary>
        /// 备注（大文本，支持详细说明）
        /// </summary>
        public string note { get; set; }

        /// <summary>
        /// 批次创建时间
        /// </summary>
        [Required]
        public DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// 批次更新时间（自动更新）
        /// </summary>
        [Required]
        public DateTime updated_at { get; set; } = DateTime.Now;

        // 导航属性
        public ProductTaskModel ProductTask { get; set; }
        public TeamModel Team { get; set; }
    }
}