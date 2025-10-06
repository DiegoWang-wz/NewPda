using DexRobotPDA.DataModel;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using Microsoft.Data.SqlClient;

namespace DexRobotPDA.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FingerController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<FingerController> _logger;

    public FingerController(DailyDbContext _db, IMapper _mapper, ILogger<FingerController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult GetFingers()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Fingers.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<FingerDto> fingers = mapper.Map<List<FingerDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = fingers;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", fingers.Count);
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
    public async Task<IActionResult> GetFinger(string finger_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var finger = await db.Fingers.FirstOrDefaultAsync(m => m.finger_id == finger_id);
            FingerDto Finger = mapper.Map<FingerDto>(finger);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Finger;
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "Error";
            _logger.LogError(e, "获取列表时发生错误");
        }

        return Ok(response);
    }
    [HttpPost]
    public async Task<IActionResult> AddFinger(AddFingerDto addFingerDto)
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

            // // 2.1 检查手指外壳是否存在
            // bool fingerShellExists = await db.Materials.AnyAsync(f => f.material_id == addFingerDto.finger_id);
            // if (!fingerShellExists)
            // {
            //     response.ResultCode = -1;
            //     response.Msg = $"手指外壳ID '{addFingerDto.finger_id}' 不存在，请检查输入数据";
            //     _logger.LogWarning("新增手指失败：手指ID不存在 - {FingerId}", addFingerDto.finger_id);
            //     return BadRequest(response);
            // }

            // 2.2 检查手指ID是否已存在（异步）
            bool fingerExists = await db.Fingers.AnyAsync(f => f.finger_id == addFingerDto.finger_id);
            if (fingerExists)
            {
                response.ResultCode = -1;
                response.Msg = $"手指ID '{addFingerDto.finger_id}' 已存在，不能重复添加";
                _logger.LogWarning("新增手指失败：手指ID已存在 - {FingerId}", addFingerDto.finger_id);
                return BadRequest(response);
            }

            // 3. 检查相同task_id的手指记录数量（可根据实际业务调整上限）
            int sameTaskCount = await db.Fingers
                .Where(f => f.task_id == addFingerDto.task_id)
                .CountAsync();

            int productNum = (await db.ProductTasks
                .Where(p => p.task_id == addFingerDto.task_id)
                .Select(p => p.product_num)
                .FirstOrDefaultAsync())*5;
            
            // 假设每个任务最多允许添加5个手指，可根据实际业务修改此数值
            if (sameTaskCount >= productNum)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addFingerDto.task_id}' 的手指数量已达到上限（{productNum}个），无法继续添加";
                _logger.LogWarning("新增手指失败：任务手指数量已达上限 - TaskId: {TaskId}, 当前数量: {Count}",
                    addFingerDto.task_id, sameTaskCount);
                return BadRequest(response);
            }

            // 4. 检查生产任务是否存在
            bool taskExists = await db.ProductTasks.AnyAsync(t => t.task_id == addFingerDto.task_id);
            if (!taskExists)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addFingerDto.task_id}' 不存在，请先创建该生产任务";
                _logger.LogWarning("新增手指失败：生产任务不存在 - TaskId: {TaskId}", addFingerDto.task_id);
                return BadRequest(response);
            }

            // 5. 检查操作员是否存在
            bool operatorExists = await db.Employees.AnyAsync(e => e.employee_id == addFingerDto.operator_id);
            if (!operatorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"操作员ID '{addFingerDto.operator_id}' 不存在，请检查操作员信息";
                _logger.LogWarning("新增手指失败：操作员不存在 - OperatorId: {OperatorId}", addFingerDto.operator_id);
                return BadRequest(response);
            }

            // 6. 使用 AutoMapper 将 DTO 转换为 Model
            var fingerModel = mapper.Map<FingerModel>(addFingerDto);

            // 7. 补充DTO中未包含但Model需要的字段
            fingerModel.palm_id = null; // 新增时未绑定手掌
            fingerModel.updated_at = DateTime.Now; // 设置更新时间

            // 8. 添加到数据库（异步）
            await db.Fingers.AddAsync(fingerModel);
            await db.SaveChangesAsync();

            // 9. 记录日志
            _logger.LogInformation("成功新增手指，手指ID: {FingerId}, 任务ID: {TaskId}, 当前任务手指数量: {Count}",
                addFingerDto.finger_id, addFingerDto.task_id, sameTaskCount + 1);

            // 10. 构建响应
            response.ResultCode = 1;
            response.Msg = "新增成功";
            response.ResultData = new
            {
                finger = mapper.Map<FingerDto>(fingerModel),
                current_count = sameTaskCount + 1,
                max_allowed = productNum,
                remaining_slots = productNum - (sameTaskCount + 1)
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            if (dbEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                response.ResultCode = -1;
                response.Msg = $"手指ID '{addFingerDto.finger_id}' 已存在（数据库约束）";
                _logger.LogWarning("新增手指失败：数据库唯一约束违反 - {FingerId}", addFingerDto.finger_id);
            }
            else
            {
                response.ResultCode = -1;
                response.Msg = "数据库操作失败";
                _logger.LogError(dbEx, "新增手指时数据库操作失败，手指ID: {FingerId}", addFingerDto?.finger_id);
            }

            return BadRequest(response);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "新增失败";

            _logger.LogError(e, "新增手指时发生错误，手指ID: {FingerId}", addFingerDto?.finger_id);

            return StatusCode(500, response);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFingerList(string taskId)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 验证任务ID是否为空
            if (string.IsNullOrEmpty(taskId))
            {
                response.ResultCode = -2;
                response.Msg = "任务ID不能为空";
                _logger.LogWarning("获取手指列表失败：任务ID为空");
                return BadRequest(response);
            }

            // 异步查询指定任务ID的手指列表
            var list = await db.Fingers
                .Where(f => f.task_id == taskId)
                .ToListAsync();

            _logger.LogDebug("从数据库获取到任务{TaskId}的手指记录{Count}条", taskId, list.Count);

            // 映射为DTO列表
            List<FingerDto> fingers = mapper.Map<List<FingerDto>>(list);

            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = fingers;

            // 记录成功信息
            _logger.LogInformation("成功获取任务{TaskId}的手指列表，共{Count}条记录", taskId, fingers.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取手指列表失败";

            // 记录错误信息，包括异常详情和任务ID
            _logger.LogError(e, "获取任务{TaskId}的手指列表时发生错误", taskId);
        }

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> FingerBindPalm(FingerBindPalmDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var finger = await db.Fingers
                .FirstOrDefaultAsync(t => t.finger_id == dto.finger_id);
            // var palm = await db.Materials
            //     .FirstOrDefaultAsync(t => t.material_id == dto.palm_id);
            if (finger == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指外壳不存在";
                return NotFound(res);
            }

            // if (palm == null)
            // {
            //     res.ResultCode = -1;
            //     res.Msg = "手掌外壳不存在";
            //     return NotFound(res);
            // }

            finger.palm_id = dto.palm_id;
            finger.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "绑定成功";
            res.ResultData = finger;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"绑定失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateQualify(UpdateQualifyDto qualifyDto)
    {
        var res = new ApiResponse();

        try
        {
            var finger = await db.Fingers
                .FirstOrDefaultAsync(t => t.finger_id == qualifyDto.id);

            if (finger == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指外壳不存在";
                return NotFound(res);
            }


            finger.is_qualified = qualifyDto.qualified;
            finger.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "更新成功";
            res.ResultData = finger;

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
    public async Task<ActionResult<ApiResponse>> GetFingerDetail(string finger_id)
    {
        var res = new ApiResponse();

        try
        {
            // 查询手指信息
            var finger = await db.Fingers
                .FirstOrDefaultAsync(t => t.finger_id == finger_id);

            if (finger == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指外壳不存在";
                return NotFound(res);
            }

            var motorIds = await db.Motors
                .Where(m => m.finger_id == finger_id)
                .Select(m => m.motor_id)
                .ToListAsync();

            var allIds = motorIds.Concat(new[] { finger.remarks ?? "" }).ToArray();

            res.ResultCode = 1;
            res.Msg = "查询成功";
            res.ResultData = allIds;

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
    public async Task<ActionResult<ApiResponse>> UnBindFinger(string finger_id)
    {
        var res = new ApiResponse();

        try
        {
            var finger = await db.Fingers
                .FirstOrDefaultAsync(t => t.finger_id == finger_id);

            if (finger == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指外壳不存在";
                return NotFound(res);
            }
            
            var palm = await db.Palms
                .FirstOrDefaultAsync(t => t.palm_id == finger.palm_id);

            if (palm != null)
            {
                palm.is_qualified = false;
            }


            finger.task_id = "";
            finger.palm_id = "";
            finger.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "手指外壳解绑成功";
            res.ResultData = finger;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"手指外壳解绑失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> ReBindFinger(ReBindDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var finger = await db.Fingers
                .FirstOrDefaultAsync(t => t.finger_id == dto.part_id);

            if (finger == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指不存在";
                return NotFound(res);
            }

            finger.task_id = dto.task_id;
            finger.palm_id = dto.on_part_id;
            finger.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "手指更新成功";
            res.ResultData = finger;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"手指解绑失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateFinger(FingerDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var finger = await db.Fingers
                .FirstOrDefaultAsync(t => t.finger_id == dto.finger_id);

            if (finger == null)
            {
                res.ResultCode = -1;
                res.Msg = "手指不存在";
                return NotFound(res);
            }
            

            finger.task_id = dto.task_id;
            finger.operator_id = dto.operator_id;
            finger.is_qualified = dto.is_qualified;
            finger.remarks = dto.remarks;
            finger.palm_id = dto.palm_id;
            finger.updated_at = DateTime.Now;
            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "手指更新成功";
            res.ResultData = finger;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"手指更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

}