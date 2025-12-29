using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;

public interface IDX023Service
{
    Task<List<ServoDto>> GetAllAsync(CancellationToken ct = default);
    Task<ServoDto?> GetServoByIdAsync(string servo_id, CancellationToken ct = default);

    Task<ApiResponse<bool>> UnbindServoAsync(string servo_id, CancellationToken ct = default);
    Task<ApiResponse<bool>> RebindServoAsync(RebindServoDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> AddServoAsync(AddServoDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> ServoBindFingerAsync(ServoBindFingerDto dto, CancellationToken ct = default);

    Task<List<ServoDto>> GetFingerDetailAsync(string superior_id, CancellationToken ct = default);
}