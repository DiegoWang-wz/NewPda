using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

/// <summary>
/// 分割蜗杆检测DTO
/// 字段名严格采用小写_格式，与Model、数据库完全对齐
/// </summary>
public class SplitWormDetectCreateDto
{
    /// <summary>
    /// 分割编号（关联分割任务/产品的唯一标识，必填）
    /// </summary>
    [Required(ErrorMessage = "分割编号不能为空")]
    [MaxLength(64, ErrorMessage = "分割编号长度不能超过64个字符")]
    public string? finger_id { get; set; } 

    /// <summary>
    /// 粘结时间
    /// </summary>
    public DateTime? combine_time { get; set; } 

    /// <summary>
    /// 投入使用时间
    /// </summary>
    public DateTime? using_time { get; set; } 

    /// <summary>
    /// 检测人员（姓名/工号）
    /// </summary>
    [MaxLength(64, ErrorMessage = "检测人员名称长度不能超过64个字符")]
    public string? inspector { get; set; } 
    /// <summary>
    /// 备注信息
    /// </summary>
    public string? remarks { get; set; } 

    /// <summary>
    /// 是否合格（0=不合格，1=合格，必填）
    /// </summary>
    [Required(ErrorMessage = "请选择合格状态")]
    public bool if_qualified { get; set; } 
    
    public string? motor_id { get; set; } 
}