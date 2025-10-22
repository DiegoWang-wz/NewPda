using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs;

public class SaleOrderBindingDto
{
    [Required(ErrorMessage = "生产单号不能为空")]
    [StringLength(64, ErrorMessage = "生产单号长度不能超过64个字符")]
    public string task_id { get; set; }
    
    [Required(ErrorMessage = "销售单号不能为空")]
    public string? sale_order_number { get; set; }
}