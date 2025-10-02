using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DexRobotPDA.DataModel
{
    /// <summary>
    /// 生产班组表（存储生产班组基础信息）
    /// </summary>
    [Table("Teams")]
    public class TeamModel
    {
        /// <summary>
        /// 主键，自增ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        /// <summary>
        /// 班组ID（支持Unicode，如含特殊符号）
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string team_id { get; set; }

        /// <summary>
        /// 班组名（支持中文）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string team_name { get; set; }

        // 导航属性
        public ICollection<EmployeeModel> Employees { get; set; } = new List<EmployeeModel>();
        public ICollection<ProductionBatchModel> Batches { get; set; } = new List<ProductionBatchModel>();
    }
}