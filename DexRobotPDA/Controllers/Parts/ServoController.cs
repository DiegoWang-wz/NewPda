using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using DexRobotPDA.Services;
using Microsoft.AspNetCore.Mvc;

namespace DexRobotPDA.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ServoController : ControllerBase
    {
        private readonly IPartService _ipartService;

        public ServoController(IPartService ipartService)
        {
            _ipartService = ipartService;
        }

        [HttpGet]
        public Task<ApiResponse<List<ServoDto>>> GetAll(CancellationToken ct = default)
            => _ipartService.GetAllAsync(ct);

        [HttpGet]
        public Task<ApiResponse<List<ServoDto>>> GetServoByTaskIdAsync([FromQuery] string task_id, CancellationToken ct = default)
            => _ipartService.GetServoByTaskIdAsync(task_id, ct);

        [HttpGet]
        public Task<ApiResponse<ServoDto?>> GetServoByIdAsync([FromQuery] string servo_id, CancellationToken ct = default)
            => _ipartService.GetServoByIdAsync(servo_id, ct);

        [HttpGet]
        public Task<ApiResponse<bool>> UnbindServoAsync([FromQuery] string servo_id, CancellationToken ct = default)
            => _ipartService.UnbindServoAsync(servo_id, ct);

        [HttpPost]
        public Task<ApiResponse<bool>> RebindServoAsync([FromBody] RebindServoDto dto, CancellationToken ct = default)
            => _ipartService.RebindServoAsync(dto, ct);

        [HttpPost]
        public Task<ApiResponse<bool>> AddServo([FromBody] AddServoDto dto, CancellationToken ct = default)
            => _ipartService.AddServoAsync(dto, ct);

        [HttpPost]
        public Task<ApiResponse<bool>> ServoBindFingerAsync([FromBody] ServoBindFingerDto dto, CancellationToken ct = default)
            => _ipartService.ServoBindFingerAsync(dto, ct);

    }
}