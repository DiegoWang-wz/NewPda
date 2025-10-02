using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 物料表（存储生产所需物料基础信息及库存）
    /// </summary>
    [Table("Materials")]
    public class MaterialModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 物料唯一编号（字母数字编码，如MAT-2024-001）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string material_id { get; set; }

        /// <summary>
        /// 物料名称（支持中文，如微型舵机、高精度力传感器）
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string material_name { get; set; }

        /// <summary>
        /// 物料类型：0=未分类，1=机械零件，2=电子元件，3=传感器，4=线材，5=结构件，6=工具，7=耗材
        /// </summary>
        [Required]
        public byte material_type { get; set; }

        /// <summary>
        /// 规格型号（支持详细参数，如MG90S/扭矩2.2kg·cm）
        /// </summary>
        [MaxLength(500)]
        public string? specification { get; set; }

        /// <summary>
        /// 供应商编号（优化长度，原255→50，如SUP-2024-001）
        /// </summary>
        [MaxLength(50)]
        public string? supplier_id { get; set; }

        /// <summary>
        /// 供应商名称（支持中文，如XX电子科技有限公司）
        /// </summary>
        [MaxLength(255)]
        public string? supplier { get; set; }

        /// <summary>
        /// 当前库存数量（默认0，避免null值）
        /// </summary>
        [Required]
        public int stock_quantity { get; set; } = 0;

        /// <summary>
        /// 计量单位（如个、套、卷、米）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string unit { get; set; } = "个";

        /// <summary>
        /// 安全库存预警数量（低于此值触发预警）
        /// </summary>
        [Required]
        public int safety_stock { get; set; } = 5;

        /// <summary>
        /// 仓库位置编码（如A区-3排-2层）
        /// </summary>
        [MaxLength(100)]
        public string? location { get; set; }

        /// <summary>
        /// 物料详细描述（如材质、精度等补充信息）
        /// </summary>
        public string? description { get; set; }

        /// <summary>
        /// 库存/信息更新时间（自动更新）
        /// </summary>
        [Required]
        public DateTime updated_at { get; set; } = DateTime.Now;

        // 导航属性
        public ICollection<MotorModel> MotorsAsWormMaterial { get; set; } = new List<MotorModel>();
        public ICollection<MotorModel> MotorsAsAdhesiveMaterial { get; set; } = new List<MotorModel>();
        
        public MotorModel MotorMaterial { get; set; }
        public FingerModel FingerShellMaterial { get; set; }
        public SplitModel SplitMaterial { get; set; }
        public PalmModel PalmShellMaterial { get; set; }
        
    }
}