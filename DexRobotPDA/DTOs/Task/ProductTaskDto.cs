using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

public class AddTaskDto
{
    [Required(ErrorMessage = "任务ID不能为空")]
    [StringLength(64, ErrorMessage = "任务ID长度不能超过64个字符")]
    public string task_id { get; set; }

    [StringLength(255, ErrorMessage = "标题长度不能超过255个字符")]
    public string? title { get; set; } = "";
    public string? description { get; set; } = "";

    [StringLength(64, ErrorMessage = "负责人ID长度不能超过64个字符")]
    public string? assignee_id { get; set; } = "";

    public DateTime created_at { get; set; } = DateTime.Now;
    public DateTime updated_at { get; set; } = DateTime.Now;
    public byte process_1 { get; set; } = 0;

    public byte process_2 { get; set; } = 0;

    public byte process_3 { get; set; } = 0;

    public byte process_4 { get; set; } = 0;

    public byte process_5 { get; set; } = 0;

    public byte process_6 { get; set; } = 0;
    public byte process_7 { get; set; } = 0;
    public byte process_8 { get; set; } = 0;
    public int product_num { get; set; } = 1;
    public string? sale_order_number { get; set; }
}