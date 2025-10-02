using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs
{
    /// <summary>
    /// 更新检测记录的DTO
    /// </summary>
    public class UpdateDetect1Dto
    {
        /// <summary>
        /// 电机编号，用于匹配要更新的检测记录所属电机
        /// </summary>
        [Required(ErrorMessage = "电机编号不能为空")]
        public long id { get; set; }
        public string? motor_id { get; set; }

        public double ? distance_before { get; set; }

        public double ? force { get; set; }

        public double ? distance_after { get; set; }

        public double ? distance_result { get; set; }

        public DateTime? combine_time { get; set; }

        public DateTime? using_time { get; set; }

        public string? inspector_id { get; set; }
    
        public string? remarks { get; set; }
    
        public bool if_qualified { get; set; }

    }
}