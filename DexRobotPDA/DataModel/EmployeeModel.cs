using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 生产员工表（存储员工基础信息及所属关系）
    /// </summary>
    [Table("Employees")]
    public class EmployeeModel
    {
        // 原有属性保持不变...
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 员工ID（字母数字编码，无需Unicode）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string employee_id { get; set; }

        /// <summary>
        /// 员工姓名（支持中文）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string employee_name { get; set; }

        /// <summary>
        /// 性别：1=男，2=女
        /// </summary>
        [Required]
        public byte gender { get; set; }

        /// <summary>
        /// 出生日期（仅日期，不含时间）
        /// </summary>
        [Required]
        public DateTime birthday { get; set; }

        /// <summary>
        /// 联系电话（支持+、-等符号）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string phone { get; set; }

        /// <summary>
        /// 所属部门（支持中文）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string department { get; set; }

        /// <summary>
        /// 关联生产班组ID（与Teams.team_id类型一致）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string team_id { get; set; }

        /// <summary>
        /// 职位（支持中文，如组装工程师）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string position { get; set; }

        /// <summary>
        /// 状态：0=离职，1=在职
        /// </summary>
        [Required]
        public byte status { get; set; } = 1;
        
        /// <summary>
        /// 员工密码
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string password { get; set; }

        // 导航属性 - 修正版
        public TeamModel team { get; set; }
        
        // 一个员工可以负责多个任务，因此使用ICollection集合类型
        public ICollection<ProductTaskModel> ProductTasks { get; set; } = new List<ProductTaskModel>();
        public ICollection<MotorModel> Motors { get; set; } = new List<MotorModel>();
        public ICollection<FingerModel> Fingers { get; set; } = new List<FingerModel>();
        public ICollection<SplitModel> Splits { get; set; } = new List<SplitModel>();
        public ICollection<PalmModel> Palms { get; set; } = new List<PalmModel>();
        
        public ICollection<MotorWormDetectModel> Detect1 { get; set; } = new List<MotorWormDetectModel>();
    }
}
    