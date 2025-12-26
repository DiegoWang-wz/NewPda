using DexRobotPDA.DTOs;

namespace DexRobotPDA.Services
{
    public interface IDX023Service
    {
        Task<List<ServoDto>> GetAllAsync(CancellationToken ct = default);
        Task<bool> AddServoAsync(AddServoDto dto);
        Task<ServoDto?> GetServoByIdAsync(string servo_id, CancellationToken ct = default);
        Task<bool> UnbindServoAsync(string servo_id, CancellationToken ct = default);
        Task<bool> RebindServoAsync(RebindServoDto dto, CancellationToken ct = default);
    }
}