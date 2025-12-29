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
        private readonly IDX023Service _idx023Service;
        private readonly ILogger<ServoController> _logger;

        public ServoController(IDX023Service idx023Service, ILogger<ServoController> logger)
        {
            _idx023Service = idx023Service;
            _logger = logger;
        }

        // ==========================
        // 查询类：Controller 包一层 ApiResponse
        // ==========================
        [HttpGet]
        public async Task<ApiResponse<List<ServoDto>>> GetAll(CancellationToken ct)
        {
            _logger.LogInformation("GET /api/Servo/GetAll called");
            try
            {
                var data = await _idx023Service.GetAllAsync(ct);

                return new ApiResponse<List<ServoDto>>
                {
                    ResultCode = 1,
                    Msg = "OK",
                    ResultData = data
                };
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse<List<ServoDto>>
                {
                    ResultCode = -2,
                    Msg = "Request canceled",
                    ResultData = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET /api/Servo/GetAll failed");
                return new ApiResponse<List<ServoDto>>
                {
                    ResultCode = 0,
                    Msg = ex.Message,
                    ResultData = null
                };
            }
        }

        [HttpGet]
        public async Task<ApiResponse<ServoDto?>> GetServoByIdAsync(string servo_id, CancellationToken ct)
        {
            try
            {
                var data = await _idx023Service.GetServoByIdAsync(servo_id, ct);

                if (data == null)
                {
                    return new ApiResponse<ServoDto?>
                    {
                        ResultCode = 404,
                        Msg = "Not Found",
                        ResultData = null
                    };
                }

                return new ApiResponse<ServoDto?>
                {
                    ResultCode = 1,
                    Msg = "OK",
                    ResultData = data
                };
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse<ServoDto?>
                {
                    ResultCode = -2,
                    Msg = "Request canceled",
                    ResultData = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET /api/Servo/GetServoByIdAsync failed, servo_id={ServoId}", servo_id);
                return new ApiResponse<ServoDto?>
                {
                    ResultCode = 0,
                    Msg = ex.Message,
                    ResultData = null
                };
            }
        }

        // ==========================
        // 写入类：直接 return Service 的 ApiResponse（关键）
        // ==========================
        [HttpGet]
        public async Task<ApiResponse<bool>> UnbindServoAsync(string servo_id, CancellationToken ct = default)
        {
            try
            {
                return await _idx023Service.UnbindServoAsync(servo_id, ct);
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
        }

        [HttpPost]
        public async Task<ApiResponse<bool>> RebindServoAsync([FromBody] RebindServoDto dto, CancellationToken ct = default)
        {
            try
            {
                return await _idx023Service.RebindServoAsync(dto, ct);
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
        }

        [HttpPost]
        public async Task<ApiResponse<bool>> AddServo([FromBody] AddServoDto dto, CancellationToken ct)
        {
            _logger.LogInformation("POST /api/Servo/AddServo called, servo_id={ServoId}, task_id={TaskId}",
                dto?.servo_id, dto?.task_id);

            try
            {
                return await _idx023Service.AddServoAsync(dto, ct);
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
        }

        [HttpPost]
        public async Task<ApiResponse<bool>> ServoBindFingerAsync([FromBody] ServoBindFingerDto dto, CancellationToken ct)
        {
            try
            {
                // ✅ 直接把 service 返回给前端，Msg/ResultCode 都保留
                return await _idx023Service.ServoBindFingerAsync(dto, ct);
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
        }
    }
}
