using DexRobotPDA.DataModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using Microsoft.Data.SqlClient;

namespace DexRobotPDA.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class Detect4Controller : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<Detect4Controller> _logger;

    public Detect4Controller(DailyDbContext _db, IMapper _mapper, ILogger<Detect4Controller> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有手指校准检测记录
    /// </summary>
    [HttpGet]
    public IActionResult GetDetect4()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 替换为 FingerCalibrateDetectModel 表
            var list = db.Detect4.ToList();
            _logger.LogDebug("从数据库获取到{Count}条手指校准检测记录", list.Count);

            // 映射为 FingerCalibrateDetectDto
            List<FingerCalibrateDetectDto> fingerDtos = mapper.Map<List<FingerCalibrateDetectDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = fingerDtos;

            _logger.LogInformation("成功获取手指校准检测记录，共{Count}条", fingerDtos.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取手指校准检测记录失败";
            _logger.LogError(e, "获取手指校准检测记录列表时发生错误");
        }

        return Ok(response);
    }

    /// <summary>
    /// 根据手指ID获取最新的校准检测记录
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFingerCalibrateDetect(string finger_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 验证输入参数（替换为 finger_id）
            if (string.IsNullOrEmpty(finger_id))
            {
                response.ResultCode = -1;
                response.Msg = "手指ID不能为空";
                _logger.LogWarning("获取检测记录失败：手指ID为空");
                return BadRequest(response);
            }

            // 2. 根据 finger_id 查询最新记录（按id降序取第一条）
            var latestDetect = await db.Detect4
                .Where(d => d.finger_id == finger_id)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            // 3. 处理查询结果
            if (latestDetect == null)
            {
                response.ResultCode = 0;
                response.Msg = $"未找到手指ID为 '{finger_id}' 的校准检测记录";
                _logger.LogInformation("未找到手指 {FingerId} 的校准检测记录", finger_id);
            }
            else
            {
                // 映射为 FingerCalibrateDetectDto 返回
                var detectDto = mapper.Map<FingerCalibrateDetectDto>(latestDetect);
                response.ResultCode = 1;
                response.Msg = "Success";
                response.ResultData = detectDto;
                _logger.LogInformation("成功获取手指 {FingerId} 的最新校准检测记录，检测ID: {DetectId}",
                    finger_id, latestDetect.id);
            }
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取手指校准检测记录失败";
            _logger.LogError(e, "获取手指 {FingerId} 的校准检测记录时发生错误", finger_id);
        }

        return Ok(response);
    }

    /// <summary>
    /// 根据任务ID获取关联的手指校准检测记录（需确保 Motors 表存在 finger_id 关联）
    /// </summary>
    [HttpGet]
    public IActionResult GetFingerCalibrateDetectList(string task_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 验证任务ID
            if (string.IsNullOrEmpty(task_id))
            {
                response.ResultCode = -1;
                response.Msg = "任务ID不能为空";
                _logger.LogWarning("获取检测记录列表失败：任务ID为空");
                return BadRequest(response);
            }

            // 关联 Motors 表查询（假设 Motors 表有 finger_id 和 task_id 字段）
            var list = db.Detect4
                .Join(db.Motors,
                    detect => detect.finger_id,
                    motor => motor.finger_id, // 需确保 Motors 表存在 finger_id 字段关联
                    (detect, motor) => new { Detect = detect, Motor = motor })
                .Where(x => x.Motor.task_id == task_id)
                .Select(x => x.Detect)
                .ToList();

            _logger.LogDebug("根据任务ID {TaskId} 获取到{Count}条手指校准检测记录", task_id, list.Count);

            // 映射为 DTO 列表
            List<FingerCalibrateDetectDto> detectDtos = mapper.Map<List<FingerCalibrateDetectDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = detectDtos;

            _logger.LogInformation("成功获取任务 {TaskId} 关联的手指校准检测记录，共{Count}条", task_id, detectDtos.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取手指校准检测记录列表失败";
            _logger.LogError(e, "根据任务ID {TaskId} 获取检测记录列表时发生错误", task_id);
        }

        return Ok(response);
    }

    /// <summary>
    /// 新增手指校准检测记录
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddDetect4(FingerCalibrateDetectCreateDto addDetectDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 基础参数验证（核心字段：finger_id、test_action）
            if (string.IsNullOrEmpty(addDetectDto.finger_id))
            {
                response.ResultCode = -1;
                response.Msg = "手指ID不能为空";
                _logger.LogWarning("新增检测记录失败：手指ID为空");
                return BadRequest(response);
            }

            if (string.IsNullOrEmpty(addDetectDto.test_action))
            {
                response.ResultCode = -1;
                response.Msg = "测试动作不能为空";
                _logger.LogWarning("新增检测记录失败：测试动作为空");
                return BadRequest(response);
            }

            // 2. 检查关联的手指设备是否存在（需确保 Motors/设备表有 finger_id 字段）
            bool fingerExists = await db.Motors.AnyAsync(m => m.finger_id == addDetectDto.finger_id);
            if (!fingerExists)
            {
                response.ResultCode = -1;
                response.Msg = $"手指ID '{addDetectDto.finger_id}' 不存在，无法创建检测记录";
                _logger.LogWarning("新增检测记录失败：手指不存在 - {FingerId}", addDetectDto.finger_id);
                return BadRequest(response);
            }

            // 3. DTO 转换为实体类
            var detectModel = mapper.Map<FingerCalibrateDetectModel>(addDetectDto);

            // 4. 添加到数据库并保存
            await db.Detect4.AddAsync(detectModel);
            await db.SaveChangesAsync();

            // 5. 日志与响应
            _logger.LogInformation("成功新增手指校准检测记录，检测ID: {DetectId}, 手指ID: {FingerId}",
                detectModel.id, addDetectDto.finger_id);

            response.ResultCode = 1;
            response.Msg = "新增手指校准检测记录成功";
            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            // 处理数据库异常（外键、唯一键等）
            string errorMsg = "数据库操作失败";
            if (dbEx.InnerException is SqlException sqlEx)
            {
                if (sqlEx.Number == 547) // 外键约束错误
                {
                    errorMsg = $"关联数据不存在（手指ID: {addDetectDto?.finger_id}）";
                }
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // 唯一键冲突
                {
                    errorMsg = "该手指的当前测试记录已存在，不能重复添加";
                }
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "新增手指校准检测记录时数据库操作失败，手指ID: {FingerId}", addDetectDto?.finger_id);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.ResultCode = -1;
            response.Msg = "新增手指校准检测记录失败";
            _logger.LogError(ex, "新增手指校准检测记录时发生错误，手指ID: {FingerId}", addDetectDto?.finger_id);

            return StatusCode(500, response);
        }
    }
}

