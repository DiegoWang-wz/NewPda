using DexRobotPDA.DataModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;
using DexRobotPDA.Services;
using Microsoft.Data.SqlClient;

namespace DexRobotPDA.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PalmController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<PalmController> _logger;
    private readonly IPartService _ipartService;

    public PalmController(DailyDbContext _db, IMapper _mapper, ILogger<PalmController> logger,
        IPartService ipartService)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
        _ipartService = ipartService;
    }

    [HttpGet]
    public IActionResult GetPalms()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Palms.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<PalmDto> palms = mapper.Map<List<PalmDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = palms;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", palms.Count);
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

    [HttpGet]
    public async Task<IActionResult> GetPalm(string palm_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var palm = await db.Palms.FirstOrDefaultAsync(m => m.palm_id == palm_id);
            PalmDto palms = mapper.Map<PalmDto>(palm);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = palms;
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "Error";
            _logger.LogError(e, "获取列表时发生错误");
        }

        return Ok(response);
    }

    /// <summary>
    /// 新增手掌
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddPalm(AddPalmDto addPalmDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 验证模型有效性
            if (!ModelState.IsValid)
            {
                response.ResultCode = -2;
                response.Msg = "参数错误：" + string.Join(";",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(response);
            }

            // // 2.1 检查手掌ID是否已存在
            // bool palmShellExists = await db.Materials.AnyAsync(p => p.material_id == addPalmDto.palm_id);
            // if (!palmShellExists)
            // {
            //     response.ResultCode = -1;
            //     response.Msg = $"手掌外壳ID '{addPalmDto.palm_id}' 不存在，请检查输入数据";
            //     _logger.LogWarning("新增手指失败：手掌ID不存在 - {palm_id}", addPalmDto.palm_id);
            //     return BadRequest(response);
            // }

            // 2.2 检查手掌ID是否已存在
            bool palmExists = await db.Palms.AnyAsync(p => p.palm_id == addPalmDto.palm_id);
            if (palmExists)
            {
                response.ResultCode = -1;
                response.Msg = $"手掌ID '{addPalmDto.palm_id}' 已存在，不能重复添加";
                _logger.LogWarning("新增手掌失败：手掌ID已存在 - {PalmId}", addPalmDto.palm_id);
                return BadRequest(response);
            }

            // 3. 检查相同task_id的手掌记录数量
            int sameTaskCount = await db.Palms
                .Where(p => p.task_id == addPalmDto.task_id)
                .CountAsync();

            int productNum = await db.ProductTasks
                .Where(p => p.task_id == addPalmDto.task_id)
                .Select(p => p.product_num)
                .FirstOrDefaultAsync();

            // 假设每个任务最多允许添加2个手掌，可根据实际业务修改
            if (sameTaskCount >= productNum)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addPalmDto.task_id}' 的手掌数量已达到上限（{productNum}个），无法继续添加";
                _logger.LogWarning("新增手掌失败：任务手掌数量已达上限 - TaskId: {TaskId}, 当前数量: {Count}",
                    addPalmDto.task_id, sameTaskCount);
                return BadRequest(response);
            }

            // 4. 检查生产任务是否存在
            bool taskExists = await db.ProductTasks.AnyAsync(t => t.task_id == addPalmDto.task_id);
            if (!taskExists)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addPalmDto.task_id}' 不存在，请先创建该生产任务";
                _logger.LogWarning("新增手掌失败：生产任务不存在 - TaskId: {TaskId}", addPalmDto.task_id);
                return BadRequest(response);
            }

            // 5. 检查操作员是否存在
            bool operatorExists = await db.Employees.AnyAsync(e => e.employee_id == addPalmDto.operator_id);
            if (!operatorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"操作员ID '{addPalmDto.operator_id}' 不存在，请检查操作员信息";
                _logger.LogWarning("新增手掌失败：操作员不存在 - OperatorId: {OperatorId}", addPalmDto.operator_id);
                return BadRequest(response);
            }

            // 6. 使用AutoMapper将DTO转换为Model
            var palmModel = mapper.Map<PalmModel>(addPalmDto);

            // 7. 补充DTO中未包含但Model需要的字段
            palmModel.updated_at = DateTime.Now;

            // 8. 添加到数据库
            await db.Palms.AddAsync(palmModel);
            await db.SaveChangesAsync();

            // 9. 记录日志
            _logger.LogInformation("成功新增手掌，手掌ID: {PalmId}, 任务ID: {TaskId}, 当前任务手掌数量: {Count}",
                addPalmDto.palm_id, addPalmDto.task_id, sameTaskCount + 1);

            // 10. 构建响应
            response.ResultCode = 1;
            response.Msg = "新增成功";
            response.ResultData = new
            {
                palm = mapper.Map<PalmDto>(palmModel),
                current_count = sameTaskCount + 1,
                max_allowed = productNum,
                remaining_slots = productNum - (sameTaskCount + 1)
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            // 处理数据库唯一约束违反
            if (dbEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                response.ResultCode = -1;
                response.Msg = $"手掌ID '{addPalmDto.palm_id}' 已存在（数据库约束）";
                _logger.LogWarning("新增手掌失败：数据库唯一约束违反 - {PalmId}", addPalmDto.palm_id);
            }
            else
            {
                response.ResultCode = -1;
                response.Msg = "数据库操作失败";
                _logger.LogError(dbEx, "新增手掌时数据库操作失败，手掌ID: {PalmId}", addPalmDto?.palm_id);
            }

            return BadRequest(response);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "新增失败";

            _logger.LogError(e, "新增手掌时发生错误，手掌ID: {PalmId}", addPalmDto?.palm_id);

            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// 根据任务ID获取手掌列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPalmList(string taskId)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 验证任务ID是否为空
            if (string.IsNullOrEmpty(taskId))
            {
                response.ResultCode = -2;
                response.Msg = "任务ID不能为空";
                _logger.LogWarning("获取手掌列表失败：任务ID为空");
                return BadRequest(response);
            }

            // 异步查询指定任务ID的手掌列表
            var list = await db.Palms
                .Where(p => p.task_id == taskId)
                .ToListAsync();

            _logger.LogDebug("从数据库获取到任务{TaskId}的手掌记录{Count}条", taskId, list.Count);

            // 映射为DTO列表
            List<PalmDto> palms = mapper.Map<List<PalmDto>>(list);

            // 可以根据实际业务添加其他统计信息
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = palms;
            // 记录成功信息
            _logger.LogInformation("成功获取任务{TaskId}的手掌列表，共{Count}条记录", taskId, palms.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取手掌列表失败";

            // 记录错误信息，包括异常详情和任务ID
            _logger.LogError(e, "获取任务{TaskId}的手掌列表时发生错误", taskId);
        }

        return Ok(response);
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateQualify(UpdateQualifyDto qualifyDto)
    {
        var res = new ApiResponse();

        try
        {
            var palm = await db.Palms
                .FirstOrDefaultAsync(t => t.palm_id == qualifyDto.id);

            if (palm == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指外壳不存在";
                return NotFound(res);
            }


            palm.is_qualified = qualifyDto.qualified;
            palm.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "更新成功";
            res.ResultData = palm;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetPalmDetail(string palm_id)
    {
        var res = new ApiResponse();

        try
        {
            // 查询手指信息
            var palm = await db.Palms
                .FirstOrDefaultAsync(t => t.palm_id == palm_id);

            if (palm == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指外壳不存在";
                return NotFound(res);
            }


            var fingerIds = await db.Fingers
                .Where(m => m.palm_id == palm_id)
                .Select(m => m.finger_id)
                .ToListAsync();


            res.ResultCode = 1;
            res.Msg = "查询成功";
            res.ResultData = fingerIds;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"查询失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UnBindPalm(string palm_id)
    {
        var res = new ApiResponse();

        try
        {
            var palm = await db.Palms
                .FirstOrDefaultAsync(t => t.palm_id == palm_id);

            if (palm == null)
            {
                res.ResultCode = -1;
                res.Msg = "手掌外壳不存在";
                return NotFound(res);
            }

            palm.task_id = "";
            palm.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "手掌解绑成功";
            res.ResultData = palm;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"手掌解绑失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> ReBindPalm(ReBindDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var palm = await db.Fingers
                .FirstOrDefaultAsync(t => t.palm_id == dto.part_id);

            if (palm == null)
            {
                res.ResultCode = -1;
                res.Msg = "手掌不存在";
                return NotFound(res);
            }

            palm.task_id = dto.task_id;
            palm.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "手掌更新成功";
            res.ResultData = palm;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"手掌更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdatePalm(PalmDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var palm = await db.Fingers
                .FirstOrDefaultAsync(t => t.palm_id == dto.palm_id);

            if (palm == null)
            {
                res.ResultCode = -1;
                res.Msg = "手掌不存在";
                return NotFound(res);
            }

            palm.task_id = dto.task_id;
            palm.operator_id = dto.operator_id;
            palm.remarks = dto.remarks;
            palm.is_qualified = dto.is_qualified;
            palm.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "手掌更新成功";
            res.ResultData = palm;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"手掌更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> AddPalmWithComponents(AddPalmWithComponentsDto dto)
    {
        var res = new ApiResponse();

        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // 1. 检查手掌是否已存在
            var existingPalm = await db.Palms
                .FirstOrDefaultAsync(p => p.palm_id == dto.palm_id);

            if (existingPalm != null)
            {
                res.ResultCode = -1;
                res.Msg = $"手掌 {dto.palm_id} 已存在";
                return BadRequest(res);
            }

            // 2. 检查是否有重复的电机ID
            var duplicateMotors = dto.component_ids
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateMotors.Any())
            {
                res.ResultCode = -1;
                res.Msg = $"存在重复的组件ID: {string.Join(", ", duplicateMotors)}";
                return BadRequest(res);
            }

            // 2. 验证所有组件是否存在且未被绑定
            foreach (var componentId in dto.component_ids)
            {
                // 检查是否为手指
                var finger = await db.Fingers
                    .FirstOrDefaultAsync(f => f.finger_id == componentId);

                if (finger != null)
                {
                    if (!string.IsNullOrEmpty(finger.palm_id))
                    {
                        res.ResultCode = -1;
                        res.Msg = $"手指 {componentId} 已被绑定";
                        return BadRequest(res);
                    }

                    if (finger.is_qualified == false)
                    {
                        res.ResultCode = -1;
                        res.Msg = $"手指 {componentId} 不合格";
                        return BadRequest(res);
                    }

                    continue;
                }


                // 组件不存在
                res.ResultCode = -1;
                res.Msg = $"组件 {componentId} 不存在";
                return BadRequest(res);
            }

            // 3. 创建手掌对象
            var palmModel = new PalmModel
            {
                palm_id = dto.palm_id,
                task_id = dto.task_id,
                operator_id = dto.operator_id,
                remarks = dto.remarks,
                is_qualified = false,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            await db.Palms.AddAsync(palmModel);

            // 4. 绑定所有组件
            foreach (var componentId in dto.component_ids)
            {
                // 绑定手指
                var finger = await db.Fingers
                    .FirstOrDefaultAsync(f => f.finger_id == componentId);

                if (finger != null)
                {
                    finger.task_id = dto.task_id;
                    finger.palm_id = dto.palm_id;
                    finger.updated_at = DateTime.Now;
                }
            }

            // 5. 保存所有更改
            await db.SaveChangesAsync();

            // 6. 提交事务
            await transaction.CommitAsync();

            res.ResultCode = 1;
            res.Msg = "手掌创建并绑定组件成功";
            res.ResultData = palmModel;
            res.ResultData = new
            {
                palm = mapper.Map<PalmDto>(palmModel),
                fingerCount = dto.component_ids.Count
            };

            _logger.LogInformation("手掌创建并绑定组件成功 - 手掌ID: {PalmId}, 组件数量: {ComponentCount}",
                dto.palm_id, dto.component_ids.Count);

            return Ok(res);
        }
        catch (Exception ex)
        {
            // 回滚事务
            await transaction.RollbackAsync();

            _logger.LogError(ex, "手掌创建并绑定组件失败 - 手掌ID: {PalmId}", dto.palm_id);

            res.ResultCode = -1;
            res.Msg = $"操作失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpGet]
    public Task<ApiResponse<List<PalmDto>>> GetPalmByTaskId(string task_id,
        CancellationToken ct = default)
        => _ipartService.GetPalmByTaskId(task_id, ct);

    [HttpPut]
    public Task<ApiResponse<bool>> NewUpdatePalm(UpdatePalmDto dto,
        CancellationToken ct = default) => _ipartService.UpdatePalm(dto, ct);
}