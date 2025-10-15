using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs
{
    /// <summary>
    /// 更新检测记录的DTO
    /// </summary>
    public class MotorDetectCreateDto
    {
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