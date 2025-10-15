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
public class Detect2Controller : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<Detect2Controller> _logger;

    public Detect2Controller(DailyDbContext _db, IMapper _mapper, ILogger<Detect2Controller> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSplitWormDetect(string finger_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 验证输入参数
            if (string.IsNullOrEmpty(finger_id))
            {
                response.ResultCode = -1;
                response.Msg = "分指机构ID不能为空";
                _logger.LogWarning("获取检测记录失败：分指机构ID为空");
                return BadRequest(response);
            }

            // 2. 根据finger_id查询，按id降序排序取第一条（最新记录）
            var latestDetect = await db.Detect2
                .Where(d => d.finger_id == finger_id)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            // 3. 处理查询结果
            if (latestDetect == null)
            {
                response.ResultCode = 0;
                response.Msg = $"未找到分指机构ID为 '{finger_id}' 的检测记录";
                _logger.LogInformation("未找到分指机构 {SplitId} 的检测记录", finger_id);
            }
            else
            {
                // 映射为DTO返回
                var detectDto = mapper.Map<SplitWormDetectDto>(latestDetect);
                response.ResultCode = 1;
                response.Msg = "Success";
                response.ResultData = detectDto;
                _logger.LogInformation("成功获取分指机构 {SplitId} 的最新检测记录，检测ID: {DetectId}",
                    finger_id, latestDetect.id);
            }
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取检测记录失败";
            _logger.LogError(e, "获取分指机构 {SplitId} 的检测记录时发生错误", finger_id);
        }

        return Ok(response);
    }

    [HttpGet]
    public IActionResult GetSplitWormDetectList(string task_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 根据task_id查询相关的SplitWormDetect记录
            var list = db.Detect2
                .Join(db.Fingers,
                    detect => detect.finger_id,
                    split => split.finger_id,
                    (detect, split) => new { Detect = detect, Split = split })
                .Where(x => x.Split.task_id == task_id)
                .Select(x => x.Detect)
                .ToList();

            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<SplitWormDetectDto> Detects = mapper.Map<List<SplitWormDetectDto>>(list);
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
    public async Task<IActionResult> AddDetect2(SplitWormDetectCreateDto addDetectDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 基础参数验证
            if (string.IsNullOrEmpty(addDetectDto.finger_id))
            {
                response.ResultCode = -1;
                response.Msg = "分指机构ID不能为空";
                _logger.LogWarning("新增检测记录失败：分指机构ID为空");
                return BadRequest(response);
            }

            // 2. 检查关联的分指机构是否存在（确保外键有效）
            bool motorExists = await db.Fingers.AnyAsync(m => m.finger_id == addDetectDto.finger_id);
            if (!motorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"分指机构ID '{addDetectDto.finger_id}' 不存在，无法创建检测记录";
                _logger.LogWarning("新增检测记录失败：分指机构不存在 - {MotorId}", addDetectDto.finger_id);
                return BadRequest(response);
            }

            // 3. 使用AutoMapper将DTO转换为实体
            var detectModel = mapper.Map<SplitWormDetectModel>(addDetectDto);

            // 5. 添加到数据库并保存
            await db.Detect2.AddAsync(detectModel);
            await db.SaveChangesAsync();

            // 6. 记录成功日志
            _logger.LogInformation("成功新增检测记录，检测ID: {DetectId}, 分指机构ID: {MotorId}",
                detectModel.id, addDetectDto.finger_id);

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
                // 外键约束错误（例如关联的分指机构不存在，虽然上面已做检查，但防止并发问题）
                if (sqlEx.Number == 547)
                {
                    errorMsg = $"关联数据不存在（分指机构ID: {addDetectDto.finger_id}）";
                }
                // 唯一键约束错误（如果表中有唯一索引）
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    errorMsg = "检测记录已存在，不能重复添加";
                }
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "新增检测记录时数据库操作失败，分指机构ID: {MotorId}", addDetectDto?.finger_id);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            // 处理其他未知异常
            response.ResultCode = -1;
            response.Msg = "新增检测记录失败";
            _logger.LogError(ex, "新增检测记录时发生错误，分指机构ID: {MotorId}", addDetectDto?.finger_id);

            return StatusCode(500, response);
        }
    }
}