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

        [HttpGet]
        public async Task<ApiResponse<List<ServoDto>>> GetAll(CancellationToken ct)
        {
            _logger.LogInformation("GET /api/servos called");
            try
            {
                var data = await _idx023Service.GetAllAsync(ct);
                _logger.LogInformation("GET /api/servos success, count={Count}", data.Count);

                return new ApiResponse<List<ServoDto>>
                {
                    ResultCode = 0,
                    Msg = "OK",
                    ResultData = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET /api/servos failed");
                return new ApiResponse<List<ServoDto>>
                {
                    ResultCode = -1,
                    Msg = ex.Message,
                    ResultData = null
                };
            }
        }
        
        [HttpGet]
        public async Task<ApiResponse<ServoDto?>> GetServoByIdAsync(string servo_id, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var data = await _idx023Service.GetServoByIdAsync(servo_id, ct);

                sw.Stop();

                if (data == null)
                {
                    _logger.LogWarning("GET /api/servos/{ServoId} not found", servo_id);
                    return new ApiResponse<ServoDto?>
                    {
                        ResultCode = 404,
                        Msg = "Not Found",
                        ResultData = null
                    };
                }

                _logger.LogInformation("GET /api/servos/{ServoId} success", servo_id);

                return new ApiResponse<ServoDto?>
                {
                    ResultCode = 0,
                    Msg = "OK",
                    ResultData = data
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "GET /api/servos/{ServoId} failed", servo_id);

                return new ApiResponse<ServoDto?>
                {
                    ResultCode = -1,
                    Msg = ex.Message,
                    ResultData = null
                };
            }
        }

        [HttpGet]
        public async Task<ApiResponse<bool>> UnbindServoAsync(string servo_id, CancellationToken ct = default)
        {
            try
            {
                var ok = await _idx023Service.UnbindServoAsync(servo_id , ct);

                _logger.LogInformation("GET /api/Servo/UnbindServoAsync success, ok={Ok}", ok);

                return new ApiResponse<bool>
                {
                    ResultCode = 0,
                    Msg = "OK",
                    ResultData = ok
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GET /api/Servo/UnbindServoAsync canceled");

                return new ApiResponse<bool>
                {
                    ResultCode = -2, 
                    Msg = "Request canceled",
                    ResultData = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST /api/Servo/UnbindServoAsync failed");

                return new ApiResponse<bool>
                {
                    ResultCode = -1,
                    Msg = ex.Message,
                    ResultData = false
                };
            }
        }
        
        [HttpPost]
        public async Task<ApiResponse<bool>> RebindServoAsync([FromBody] RebindServoDto dto, CancellationToken ct = default)
        {
            try
            {
                var ok = await _idx023Service.RebindServoAsync(dto , ct);

                _logger.LogInformation("POST /api/Servo/RebindServoAsync success, ok={Ok}", ok);

                return new ApiResponse<bool>
                {
                    ResultCode = 0,
                    Msg = "OK",
                    ResultData = ok
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("POST /api/Servo/RebindServoAsync canceled");

                return new ApiResponse<bool>
                {
                    ResultCode = -2,
                    Msg = "Request canceled",
                    ResultData = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST /api/Servo/RebindServoAsync failed");

                return new ApiResponse<bool>
                {
                    ResultCode = -1,
                    Msg = ex.Message,
                    ResultData = false
                };
            }
        }
        
        [HttpPost]
        public async Task<ApiResponse<bool>> AddServo([FromBody] AddServoDto dto, CancellationToken ct)
        {
            _logger.LogInformation("POST /api/Servo/AddServo called, servo_id={ServoId}, task_id={TaskId}",
                dto?.servo_id, dto?.task_id);

            try
            {
                // 这里调用你的写入方法
                var ok = await _idx023Service.AddServoAsync(dto /*, ct 如果你给 service 加上 ct */);

                _logger.LogInformation("POST /api/Servo/AddServo success, ok={Ok}", ok);

                return new ApiResponse<bool>
                {
                    ResultCode = 0,
                    Msg = "OK",
                    ResultData = ok
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("POST /api/Servo/AddServo canceled");

                return new ApiResponse<bool>
                {
                    ResultCode = -2,
                    Msg = "Request canceled",
                    ResultData = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST /api/Servo/AddServo failed");

                return new ApiResponse<bool>
                {
                    ResultCode = -1,
                    Msg = ex.Message,
                    ResultData = false
                };
            }
        }
    }
}