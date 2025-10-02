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
public class ProductTaskController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<ProductTaskController> _logger;

    public ProductTaskController(DailyDbContext _db, IMapper _mapper, ILogger<ProductTaskController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetTaskList()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.ProductTasks.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<ProductTaskDto> Tasks = mapper.Map<List<ProductTaskDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Tasks;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Tasks.Count);
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
    public IActionResult GetTaskDetail(string taskId)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var task = db.ProductTasks.FirstOrDefault(b => b.task_id == taskId);

            if (task == null)
            {
                response.ResultCode = 0;
                response.Msg = "任务不存在";
                return Ok(response);
            }

            ProductTaskDto taskDto = mapper.Map<ProductTaskDto>(task);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = taskDto;
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = $"Error: {e.Message}";
            // 记录日志
            _logger.LogError(e, "获取任务详情失败，任务ID: {TaskId}", taskId);
        }

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> AddTask(AddTaskDto taskDto)
    {
        var res = new ApiResponse();

        // 1. 数据验证（ModelState会自动验证DataAnnotation）
        if (!ModelState.IsValid)
        {
            res.ResultCode = -1;
            res.Msg = "数据验证失败";
            res.ResultData = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(res);
        }

        try
        {
            // 2. 检查task_id是否唯一（添加索引查询优化）
            bool taskIdExists = await db.ProductTasks
                .AsNoTracking()
                .AnyAsync(p => p.task_id == taskDto.task_id);

            if (taskIdExists)
            {
                res.ResultCode = -2;
                res.Msg = $"任务ID '{taskDto.task_id}' 已存在";
                return Conflict(res);
            }

            // 3. 验证负责人是否存在（如果提供了assignee_id）
            if (!string.IsNullOrEmpty(taskDto.assignee_id))
            {
                bool assigneeExists = await db.Employees
                    .AsNoTracking()
                    .AnyAsync(e => e.employee_id == taskDto.assignee_id);

                if (!assigneeExists)
                {
                    res.ResultCode = -3;
                    res.Msg = $"负责人ID '{taskDto.assignee_id}' 不存在";
                    return BadRequest(res);
                }
            }

            // 4. 使用AutoMapper映射（确保配置了忽略id和正确的时间处理）
            ProductTaskModel newTask = mapper.Map<ProductTaskModel>(taskDto);

            // 5. 确保时间正确（覆盖DTO中的时间，使用服务器时间）
            newTask.created_at = DateTime.Now;
            newTask.updated_at = DateTime.Now;

            // 6. 添加并保存
            db.ProductTasks.Add(newTask);
            int result = await db.SaveChangesAsync();

            if (result > 0)
            {
                res.ResultCode = 1;
                res.Msg = "任务创建成功";
                res.ResultData = new
                {
                    id = newTask.id,
                    task_id = newTask.task_id,
                    created_at = newTask.created_at
                };

                // 返回201 Created状态码
                return StatusCode(200, res);
            }
            else
            {
                res.ResultCode = -99;
                res.Msg = "任务创建失败，未保存任何数据";
                return StatusCode(500, res);
            }
        }
        catch (DbUpdateException dbEx)
        {
            // 数据库异常（唯一约束冲突等）
            res.ResultCode = -98;
            res.Msg = "数据库操作失败";

            // 记录详细日志
            _logger.LogError(dbEx, "数据库操作异常，任务ID: {TaskId}", taskDto.task_id);

            return StatusCode(500, res);
        }
        catch (Exception ex)
        {
            // 其他异常
            res.ResultCode = -99;
            res.Msg = "系统内部错误";

            // 记录日志
            _logger.LogError(ex, "创建生产任务失败，任务ID: {TaskId}", taskDto.task_id);

            return StatusCode(500, res);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> UpdateTaskProcessStatus([FromBody] UpdateTaskProcessStatusDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == dto.task_id);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            switch (dto.process.ToLower())
            {
                case "process1":
                    task.process_1 = dto.status;
                    break;
                case "process2":
                    task.process_2 = dto.status;
                    break;
                case "process3":
                    task.process_3 = dto.status;
                    break;
                case "process4":
                    task.process_4 = dto.status;
                    break;
                case "process5":
                    task.process_5 = dto.status;
                    break;
                case "process6":
                    task.process_6 = dto.status;
                    break;
                case "process7":
                    task.process_7 = dto.status;
                    break;
                case "process8":
                    task.process_8 = dto.status;
                    break;
                default:
                    task.status = dto.status;
                    break;
            }

            task.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "更新成功";
            res.ResultData = task;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> UpdateTaskStatus([FromBody] UpdateTaskStatusDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == dto.task_id);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            task.status = dto.status;
            task.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "更新成功";
            res.ResultData = task;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateDetect1Status([FromQuery] string taskId)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            var qualifiedCount = db.Motors
                .Where(m => m.task_id == taskId)
                .SelectMany(m => db.Detect1.Where(d => d.motor_id == m.motor_id))
                .GroupBy(d => d.motor_id)
                .Select(g => new { MotorId = g.Key, LatestDetect = g.OrderByDescending(d => d.id).First() })
                .Count(x => x.LatestDetect.if_qualified == true);
            if (qualifiedCount == task.product_num * 11)
            {
                task.updated_at = DateTime.Now;
                task.process_2 = 1;
            }
            else
            {
                task.updated_at = DateTime.Now;
                task.process_2 = 0;
            }
            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = Convert.ToString(qualifiedCount);

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateDetect2Status(string taskId)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            var qualifiedCount = db.Splits
                .Where(m => m.task_id == taskId)
                .SelectMany(m => db.Detect2.Where(d => d.split_id == m.split_id))
                .GroupBy(d => d.split_id)
                .Select(g => new { SplitID = g.Key, LatestDetect = g.OrderByDescending(d => d.id).First() })
                .Count(x => x.LatestDetect.if_qualified == true);
            if (qualifiedCount == task.product_num)
            {
                task.updated_at = DateTime.Now;
                task.process_3 = 1;
            }
            else
            {
                task.updated_at = DateTime.Now;
                task.process_3 = 0;
            }
            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = Convert.ToString(qualifiedCount);

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateDetect3Status(string taskId)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            var qualifiedCount = db.Splits
                .Where(m => m.task_id == taskId)
                .SelectMany(m => db.Detect3.Where(d => d.split_id == m.split_id))
                .GroupBy(d => d.split_id)
                .Select(g => new { SplitID = g.Key, LatestDetect = g.OrderByDescending(d => d.id).First() })
                .Count(x => x.LatestDetect.if_qualified == true);
            if (qualifiedCount == task.product_num)
            {
                task.updated_at = DateTime.Now;
                task.process_4 = 1;
            }
            else
            {
                task.updated_at = DateTime.Now;
                task.process_4 = 0;
            }

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = Convert.ToString(qualifiedCount);

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateDetect4Status(string taskId)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            var qualifiedCount = db.Fingers
                .Where(m => m.task_id == taskId)
                .SelectMany(m => db.Detect4.Where(d => d.finger_id == m.finger_id))
                .GroupBy(d => d.finger_id)
                .Select(g => new { FingerID = g.Key, LatestDetect = g.OrderByDescending(d => d.id).First() })
                .Count(x => x.LatestDetect.if_qualified == true);
            if (qualifiedCount == task.product_num * 5)
            {
                task.updated_at = DateTime.Now;
                task.process_6 = 1;
            }
            else
            {
                task.updated_at = DateTime.Now;
                task.process_6 = 0;
            }

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = Convert.ToString(qualifiedCount);

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateDetect5Status(string taskId)
    {
        var res = new ApiResponse();

        try
        {
            var task = await db.ProductTasks
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
            {
                res.ResultCode = -1;
                res.Msg = "任务不存在";
                return NotFound(res);
            }

            var qualifiedCount = db.Palms
                .Where(m => m.task_id == taskId)
                .SelectMany(m => db.Detect5.Where(d => d.palm_id == m.palm_id))
                .GroupBy(d => d.palm_id)
                .Select(g => new { PalmID = g.Key, LatestDetect = g.OrderByDescending(d => d.id).First() })
                .Count(x => x.LatestDetect.if_qualified == true);
            if (qualifiedCount == task.product_num)
            {
                task.updated_at = DateTime.Now;
                task.process_8= 1;
                task.status = 2;
            }
            else
            {
                task.updated_at = DateTime.Now;
                task.process_8= 0;
                task.status = 1;
            }

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = Convert.ToString(qualifiedCount);

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }
}