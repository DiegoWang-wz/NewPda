namespace DexRobotPDA.DTOs;

public class ProductionBatchDto
{
    public long id { get; set; }

    public string task_id { get; set; }

    public string batch_number { get; set; }

    public int? quantity_produced { get; set; }

    public DateTime production_date { get; set; }

    public string team_id { get; set; }

    public long? equipment_id { get; set; }

    public string raw_material_batch { get; set; }

    public byte? quality_status { get; set; }

    public string note { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;
    
    public DateTime updated_at { get; set; } = DateTime.Now;
}