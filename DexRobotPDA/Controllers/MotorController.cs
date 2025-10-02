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
public class MotorController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<MotorController> _logger;

    public MotorController(DailyDbContext _db, IMapper _mapper, ILogger<MotorController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMotor(string motor_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var motor = await db.Motors.FirstOrDefaultAsync(m => m.motor_id == motor_id);
            MotorDto motors = mapper.Map<MotorDto>(motor);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = motors;
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
    public async Task<IActionResult> AddMotor(AddMotorDto addMotorDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 检查电机ID是否已存在（异步）
            bool motorExists = await db.Motors.AnyAsync(m => m.motor_id == addMotorDto.motor_id);
            if (motorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"电机ID '{addMotorDto.motor_id}' 已存在，不能重复添加";
                _logger.LogWarning("新增电机失败：电机ID已存在 - {MotorId}", addMotorDto.motor_id);
                return BadRequest(response);
            }

            // 2. 检查相同task_id的记录是否超过11个（异步）
            int sameTaskCount = await db.Motors
                .Where(m => m.task_id == addMotorDto.task_id)
                .CountAsync();

            int productNum = (await db.ProductTasks
                .Where(p => p.task_id == addMotorDto.task_id)
                .Select(p => p.product_num)
                .FirstOrDefaultAsync())*11;

            if (sameTaskCount >= productNum)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addMotorDto.task_id}' 的电机数量已达到上限，无法继续添加";
                _logger.LogWarning("新增电机失败：任务电机数量已达上限 - TaskId: {TaskId}, 当前数量: {Count}, 任务数量: {Count2}",
                    addMotorDto.task_id, sameTaskCount,productNum);
                return BadRequest(response);
            }

            // 3. 使用 AutoMapper 将 DTO 转换为 Model
            var motorModel = mapper.Map<MotorModel>(addMotorDto);

            // 4. 添加到数据库（异步）
            await db.Motors.AddAsync(motorModel);
            await db.SaveChangesAsync();

            // 5. 记录日志
            _logger.LogInformation("成功新增电机，电机ID: {MotorId}, 任务ID: {TaskId}, 当前任务电机数量: {Count}",
                addMotorDto.motor_id, addMotorDto.task_id, sameTaskCount + 1);

            // 6. 构建响应
            response.ResultCode = 1;
            response.Msg = "新增成功";
            response.ResultData = new
            {
                motor = motorModel,
                current_count = sameTaskCount + 1,
                max_allowed = 11,
                remaining_slots = 11 - (sameTaskCount + 1)
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            // 处理数据库唯一约束违反
            if (dbEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                response.ResultCode = -1;
                response.Msg = $"电机ID '{addMotorDto.motor_id}' 已存在（数据库约束）";
                _logger.LogWarning("新增电机失败：数据库唯一约束违反 - {MotorId}", addMotorDto.motor_id);
            }
            else
            {
                response.ResultCode = -1;
                response.Msg = "数据库操作失败";
                _logger.LogError(dbEx, "新增电机时数据库操作失败，电机ID: {MotorId}", addMotorDto?.motor_id);
            }

            return BadRequest(response);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "新增失败";

            _logger.LogError(e, "新增电机时发生错误，电机ID: {MotorId}", addMotorDto?.motor_id);

            return StatusCode(500, response);
        }
    }

    [HttpGet]
    public IActionResult GetFinishedList(string taskId)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Motors.Where(T => T.task_id == taskId).ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<MotorDto> motors = mapper.Map<List<MotorDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = motors;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", motors.Count);
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
    public async Task<ActionResult<ApiResponse>> MotorBindFinger(MotorBindFingerDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var motor = await db.Motors
                .FirstOrDefaultAsync(t => t.motor_id == dto.motor_id);

            // var finger = await db.Materials
            //     .FirstOrDefaultAsync(t => t.material_id == dto.finger_id);
            if (motor == null)
            {
                res.ResultCode = -1;
                res.Msg = $"{dto.motor_id},电机不存在";
                return NotFound(res);
            }

            // if (finger == null)
            // {
            //     res.ResultCode = -1;
            //     res.Msg = $"{dto.finger_id},手指外壳不存在";
            //     return NotFound(res);
            // }

            motor.finger_id = dto.finger_id;
            motor.update_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "绑定成功";
            res.ResultData = motor;

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
            var motor = await db.Motors
                .FirstOrDefaultAsync(t => t.motor_id == qualifyDto.id);

            if (motor == null)
            {
                res.ResultCode = -1;
                res.Msg = "电机不存在";
                return NotFound(res);
            }

            motor.is_qualified = qualifyDto.qualified;
            motor.update_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "更新成功";
            res.ResultData = motor;

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
    public IActionResult GetMotorQualify(string motor_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var motor =  db.Motors.FirstOrDefault(m => m.motor_id == motor_id);


            if (motor == null)
            {
                response.ResultCode = 0;
                response.Msg = "电机不存在";
                return Ok(response);
            }

            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = new QualifyDto(){
                qualify = motor.is_qualified,
                message = motor.motor_id,
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
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UnBindMotor(string motor_id)
    {
        var res = new ApiResponse();

        try
        {
            var motor = await db.Motors
                .FirstOrDefaultAsync(t => t.motor_id == motor_id);

            if (motor == null)
            {
                res.ResultCode = -1;
                res.Msg = "电机不存在";
                return NotFound(res);
            }

            motor.task_id = "";
            motor.finger_id = "";
            motor.update_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "电机解绑成功";
            res.ResultData = motor;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"电机解绑失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> ReBindMotor(ReBindDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var motor = await db.Motors
                .FirstOrDefaultAsync(t => t.motor_id == dto.part_id);

            if (motor == null)
            {
                res.ResultCode = -1;
                res.Msg = "电机不存在";
                return NotFound(res);
            }

            motor.task_id = dto.task_id;
            motor.finger_id = dto.on_part_id;
            motor.update_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "电机重绑成功";
            res.ResultData = motor;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"电机重绑失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateMotor(MotorDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var motor = await db.Motors
                .FirstOrDefaultAsync(t => t.motor_id == dto.motor_id);

            if (motor == null)
            {
                res.ResultCode = -1;
                res.Msg = "电机不存在";
                return NotFound(res);
            }

            motor.task_id = dto.task_id;
            motor.worm_material_id = dto.worm_material_id;
            motor.adhesive_material_id = dto.adhesive_material_id;
            motor.operator_id = dto.operator_id;
            motor.remarks = dto.remarks;
            motor.is_qualified = dto.is_qualified;
            motor.finger_id = dto.finger_id;
            motor.update_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "电机更新成功";
            res.ResultData = motor;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"电机更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }
}