namespace DexRobotPDA.DTOs;

public class MaterialDto
{
    public long id { get; set; }

    public string material_id { get; set; }

    public string material_name { get; set; }

    public byte material_type { get; set; }

    public string? specification { get; set; }

    public string? supplier_id { get; set; }

    public string? supplier { get; set; }

    public int stock_quantity { get; set; } = 0;

    public string unit { get; set; } = "个";

    public int safety_stock { get; set; } = 5;

    public string? location { get; set; }

    public string? description { get; set; }

    public DateTime updated_at { get; set; } = DateTime.Now;
}