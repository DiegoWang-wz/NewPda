using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 成品手指表（记录手指组装详情）
    /// </summary>
    [Table("Fingers")]
    public class FingerModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        
        [Required]
        [MaxLength(64)]
        public string finger_id { get; set; }
        
        [Required]
        [MaxLength(64)]
        public string task_id { get; set; }
        
        [Required]
        [MaxLength(64)]
        public string operator_id { get; set; }
        
        public string? remarks { get; set; }
        
        [MaxLength(64)]
        public string? palm_id { get; set; }
        
        [Required]
        public DateTime? created_at { get; set; } = DateTime.Now;
        
        public DateTime? updated_at { get; set; }

        [Required]
        public bool is_qualified { get; set; } = false;
        // 导航属性
        public ProductTaskModel TaskModel { get; set; }
        public MaterialModel FingerMaterial { get; set; }
        public EmployeeModel Operator { get; set; }
        public PalmModel Palm { get; set; }
        public ICollection<MotorModel> Motors { get; set; } = new List<MotorModel>();
    }
}