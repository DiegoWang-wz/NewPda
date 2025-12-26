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
public class Detect5Controller : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<Detect5Controller> _logger;

    public Detect5Controller(DailyDbContext _db, IMapper _mapper, ILogger<Detect5Controller> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetDetect5()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Detect5.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<PalmCalibrateDetectDto> Palms = mapper.Map<List<PalmCalibrateDetectDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Palms;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Palms.Count);
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
    public async Task<IActionResult> AddDetect5(PalmCalibrateDetectCreateDto addDetectDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 基础参数验证
            if (string.IsNullOrEmpty(addDetectDto.palm_id))
            {
                response.ResultCode = -1;
                response.Msg = "手掌外壳ID不能为空";
                _logger.LogWarning("新增检测记录失败：手掌外壳ID为空");
                return BadRequest(response);
            }

            // 2. 检查关联的手掌外壳是否存在（确保外键有效）
            bool motorExists = await db.Palms.AnyAsync(m => m.palm_id == addDetectDto.palm_id);
            if (!motorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"手掌外壳ID '{addDetectDto.palm_id}' 不存在，无法创建检测记录";
                _logger.LogWarning("新增检测记录失败：手掌外壳不存在 - {MotorId}", addDetectDto.palm_id);
                return BadRequest(response);
            }

            // 3. 使用AutoMapper将DTO转换为实体
            var detectModel = mapper.Map<PalmCalibrateDetectModel>(addDetectDto);

            // 5. 添加到数据库并保存
            await db.Detect5.AddAsync(detectModel);
            await db.SaveChangesAsync();

            // 6. 记录成功日志
            _logger.LogInformation("成功新增检测记录，检测ID: {DetectId}, 手掌外壳ID: {MotorId}",
                detectModel.id, addDetectDto.palm_id);

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
                // 外键约束错误（例如关联的手掌外壳不存在，虽然上面已做检查，但防止并发问题）
                if (sqlEx.Number == 547)
                {
                    errorMsg = $"关联数据不存在（手掌外壳ID: {addDetectDto.palm_id}）";
                }
                // 唯一键约束错误（如果表中有唯一索引）
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    errorMsg = "检测记录已存在，不能重复添加";
                }
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "新增检测记录时数据库操作失败，手掌外壳ID: {MotorId}", addDetectDto?.palm_id);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            // 处理其他未知异常
            response.ResultCode = -1;
            response.Msg = "新增检测记录失败";
            _logger.LogError(ex, "新增检测记录时发生错误，手掌外壳ID: {MotorId}", addDetectDto?.palm_id);

            return StatusCode(500, response);
        }
    }
    
    [HttpGet]
    public IActionResult GetPalmCalibrateDetectList(string task_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Detect5
                .Join(db.Palms,
                    detect => detect.palm_id,
                    palm => palm.palm_id,
                    (detect, palm) => new { Detect = detect, Palm = palm })
                .Where(x => x.Palm.task_id == task_id)
                .Select(x => x.Detect)
                .ToList();

            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<PalmCalibrateDetectDto> Detects = mapper.Map<List<PalmCalibrateDetectDto>>(list);
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
}