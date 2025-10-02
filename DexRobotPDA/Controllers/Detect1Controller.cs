using DexRobotPDA.DataModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;
using Microsoft.Data.SqlClient;

namespace DexRobotPDA.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class Detect1Controller : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<Detect1Controller> _logger;

    public Detect1Controller(DailyDbContext _db, IMapper _mapper, ILogger<Detect1Controller> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMotorWormDetect(string motor_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 验证输入参数
            if (string.IsNullOrEmpty(motor_id))
            {
                response.ResultCode = -1;
                response.Msg = "电机ID不能为空";
                _logger.LogWarning("获取检测记录失败：电机ID为空");
                return BadRequest(response);
            }

            // 2. 根据motor_id查询，按id降序排序取第一条（最新记录）
            var latestDetect = await db.Detect1
                .Where(d => d.motor_id == motor_id)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            // 3. 处理查询结果
            if (latestDetect == null)
            {
                response.ResultCode = 0;
                response.Msg = $"未找到电机ID为 '{motor_id}' 的检测记录";
                _logger.LogInformation("未找到电机 {MotorId} 的检测记录", motor_id);
            }
            else
            {
                // 映射为DTO返回
                var detectDto = mapper.Map<MotorWormDetectDto>(latestDetect);
                response.ResultCode = 1;
                response.Msg = "Success";
                response.ResultData = detectDto;
                _logger.LogInformation("成功获取电机 {MotorId} 的最新检测记录，检测ID: {DetectId}",
                    motor_id, latestDetect.id);
            }
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取检测记录失败";
            _logger.LogError(e, "获取电机 {MotorId} 的检测记录时发生错误", motor_id);
        }

        return Ok(response);
    }

    [HttpGet]
    public IActionResult GetMotorWormDetectList(string task_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 根据task_id查询相关的MotorWormDetect记录
            var list = db.Detect1
                .Join(db.Motors,
                    detect => detect.motor_id,
                    motor => motor.motor_id,
                    (detect, motor) => new { Detect = detect, Motor = motor })
                .Where(x => x.Motor.task_id == task_id)
                .Select(x => x.Detect)
                .ToList();

            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<MotorWormDetectDto> Detects = mapper.Map<List<MotorWormDetectDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Detects;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Detects.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "Error";

            // 记录错误信息，包括异常详情
            _logger.LogError(e, "获取列表时发生错误");
        }

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddDetect1(UpdateDetect1Dto addDetectDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 基础参数验证
            if (string.IsNullOrEmpty(addDetectDto.motor_id))
            {
                response.ResultCode = -1;
                response.Msg = "电机ID不能为空";
                _logger.LogWarning("新增检测记录失败：电机ID为空");
                return BadRequest(response);
            }

            // 2. 检查关联的电机是否存在（确保外键有效）
            bool motorExists = await db.Motors.AnyAsync(m => m.motor_id == addDetectDto.motor_id);
            if (!motorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"电机ID '{addDetectDto.motor_id}' 不存在，无法创建检测记录";
                _logger.LogWarning("新增检测记录失败：电机不存在 - {MotorId}", addDetectDto.motor_id);
                return BadRequest(response);
            }

            // 3. 使用AutoMapper将DTO转换为实体
            var detectModel = mapper.Map<MotorWormDetectModel>(addDetectDto);

            // 5. 添加到数据库并保存
            await db.Detect1.AddAsync(detectModel);
            await db.SaveChangesAsync();

            // 6. 记录成功日志
            _logger.LogInformation("成功新增检测记录，检测ID: {DetectId}, 电机ID: {MotorId}",
                detectModel.id, addDetectDto.motor_id);

            // 7. 构建成功响应
            response.ResultCode = 1;
            response.Msg = "新增检测记录成功";
            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            // 处理数据库相关异常（如外键约束错误）
            string errorMsg = "数据库操作失败";
            if (dbEx.InnerException is SqlException sqlEx)
            {
                // 外键约束错误（例如关联的电机不存在，虽然上面已做检查，但防止并发问题）
                if (sqlEx.Number == 547)
                {
                    errorMsg = $"关联数据不存在（电机ID: {addDetectDto.motor_id}）";
                }
                // 唯一键约束错误（如果表中有唯一索引）
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    errorMsg = "检测记录已存在，不能重复添加";
                }
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "新增检测记录时数据库操作失败，电机ID: {MotorId}", addDetectDto?.motor_id);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            // 处理其他未知异常
            response.ResultCode = -1;
            response.Msg = "新增检测记录失败";
            _logger.LogError(ex, "新增检测记录时发生错误，电机ID: {MotorId}", addDetectDto?.motor_id);

            return StatusCode(500, response);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateLatestDetect(MotorWormDetectDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            _logger.LogWarning("模型绑定失败，错误: {Errors}", string.Join(", ", errors));

            ApiResponse response1 = new ApiResponse
            {
                ResultCode = -1,
                Msg = "请求数据格式错误: " + string.Join(", ", errors)
            };
            return BadRequest(response1);
        }

        ApiResponse response = new ApiResponse();
        try
        {
            if (string.IsNullOrEmpty(dto.motor_id))
            {
                response.ResultCode = -1;
                response.Msg = "电机ID不能为空";
                _logger.LogWarning("更新检测记录失败：电机ID为空");
                return BadRequest(response);
            }

            var latestDetect = await db.Detect1
                .Where(d => d.motor_id == dto.motor_id)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            if (latestDetect == null)
            {
                response.ResultCode = -1;
                response.Msg = $"电机ID '{dto.motor_id}' 不存在检测记录，无法更新";
                _logger.LogWarning("更新检测记录失败：未找到电机 {MotorId} 的检测记录", dto.motor_id);
                return BadRequest(response);
            }

            // 直接使用DTO字段更新实体，保持字段名一致
            if (dto.distance_before.HasValue)
                latestDetect.distance_before = dto.distance_before;

            if (dto.force.HasValue)
                latestDetect.force = dto.force;

            if (dto.distance_after.HasValue)
                latestDetect.distance_after = dto.distance_after;

            if (dto.distance_result.HasValue)
                latestDetect.distance_result = dto.distance_result;
            else if (dto.distance_before.HasValue && dto.distance_after.HasValue)
                // 计算距离结果时添加四舍五入
                latestDetect.distance_result = Math.Round(
                    dto.distance_before.Value - dto.distance_after.Value,
                    2
                );

            if (dto.using_time.HasValue)
                latestDetect.using_time = dto.using_time;

            if (!string.IsNullOrEmpty(dto.inspector_id))
                latestDetect.inspector_id = dto.inspector_id;

            if (!string.IsNullOrEmpty(dto.remarks))
                latestDetect.remarks = dto.remarks;

            if (dto.if_qualified == true || dto.if_qualified == false)
            {
                latestDetect.if_qualified = dto.if_qualified;
            }
            else
            {
                // 自动判断合格状态的逻辑保持不变
                bool isQualified = false;
                if (latestDetect.distance_before.HasValue && latestDetect.distance_after.HasValue)
                {
                    double distanceDiff = latestDetect.distance_before.Value - latestDetect.distance_after.Value;
                    if (distanceDiff <= 0.02)
                    {
                        if (latestDetect.combine_time.HasValue && latestDetect.using_time.HasValue)
                        {
                            TimeSpan timeDiff = latestDetect.using_time.Value - latestDetect.combine_time.Value;
                            if (timeDiff.TotalHours > 72)
                            {
                                isQualified = true;
                            }
                        }
                    }
                }

                latestDetect.if_qualified = isQualified;
            }

            // 保存更新
            db.Detect1.Update(latestDetect);
            await db.SaveChangesAsync();

            // 记录日志并返回结果
            _logger.LogInformation("成功更新电机 {MotorId} 的最新检测记录，检测ID: {DetectId}",
                dto.motor_id, latestDetect.id);

            response.ResultCode = 1;
            response.Msg = "检测记录更新成功";
            response.ResultData = new
            {
                detect_id = latestDetect.id,
                motor_id = latestDetect.motor_id,
                if_qualified = latestDetect.if_qualified
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            string errorMsg = "数据库操作失败";
            if (dbEx.InnerException is SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "更新检测记录时发生数据库错误，错误编号: {ErrorCode}", sqlEx.Number);
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "更新电机 {MotorId} 的检测记录时数据库操作失败", dto?.motor_id);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.ResultCode = -1;
            response.Msg = "更新检测记录失败";
            _logger.LogError(ex, "更新电机 {MotorId} 的检测记录时发生错误", dto?.motor_id);

            return StatusCode(500, response);
        }
    }

    [HttpGet]
    public IActionResult Detect1Message(string motor_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var detect1 = db.Detect1.FirstOrDefault(m => m.motor_id == motor_id);
            if (detect1 == null)
            {
                response.ResultCode = 0;
                response.Msg = "检测信息不存在";
                return Ok(response);
            }

            List<string> issues = new List<string>();

            // 检查测试前距离值
            if (detect1.distance_before.HasValue && Math.Abs(detect1.distance_before.Value - 18.0) > 0.1)
            {
                issues.Add($"测试前的距离值 {detect1.distance_before.Value} 超出标准范围 (17.9-18.1)");
            }

            // 检查测试后距离值
            if (detect1.distance_after.HasValue && Math.Abs(detect1.distance_after.Value - 18.0) > 0.1)
            {
                issues.Add($"测试后的距离值 {detect1.distance_after.Value} 超出标准范围 (17.9-18.1)");
            }

            // 检查距离差
            if (detect1.distance_result.HasValue && detect1.distance_result.Value >= 0.02)
            {
                issues.Add("测试前后距离差大于等于0.02");
            }

            // 检查粘接时间
            if (detect1.combine_time.HasValue && detect1.using_time.HasValue)
            {
                TimeSpan timeDiff = detect1.using_time.Value - detect1.combine_time.Value;
                if (timeDiff.TotalHours <= 72)
                {
                    issues.Add("粘接时间小于72小时");
                }
            }

            string message = issues.Count > 0 ? "不合格原因：" + string.Join(";", issues) : "合格";

            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = new QualifyDto(){
                qualify = issues.Count <= 0,
                message = message
            };
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = $"Error: {e.Message}";
            _logger.LogError(e, "获取电机状态失败，电机ID: {motor_id}", motor_id);
        }

        return Ok(response);
    }
}