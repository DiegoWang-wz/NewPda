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
public class SplitController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<SplitController> _logger;

    public SplitController(DailyDbContext _db, IMapper _mapper, ILogger<SplitController> logger)
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
            var list = db.Splits.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<SplitDto> Splits = mapper.Map<List<SplitDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Splits;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Splits.Count);
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
    public async Task<IActionResult> GetSplit(string split_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var motor = await db.Splits.FirstOrDefaultAsync(m => m.split_id == split_id);
            SplitDto Split = mapper.Map<SplitDto>(motor);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Split;
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
    public async Task<IActionResult> AddSplit(AddSplitDto addSplitDto)
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

            // 2.1 检查分指机构物料是否存在
            bool splitMaterialExists = await db.Materials.AnyAsync(s => s.material_id == addSplitDto.split_id);
            if (!splitMaterialExists)
            {
                response.ResultCode = -1;
                response.Msg = $"分指机构ID '{addSplitDto.split_id}' 不存在，请检查输入数据";
                _logger.LogWarning("新增分指机构失败：分指机构ID不存在 - {SplitId}", addSplitDto.split_id);
                return BadRequest(response);
            }

            // 2.2 检查分指机构ID是否已存在
            bool splitExists = await db.Splits.AnyAsync(s => s.split_id == addSplitDto.split_id);
            if (splitExists)
            {
                response.ResultCode = -1;
                response.Msg = $"分指机构ID '{addSplitDto.split_id}' 已存在，不能重复添加";
                _logger.LogWarning("新增分指机构失败：分指机构ID已存在 - {SplitId}", addSplitDto.split_id);
                return BadRequest(response);
            }

            // 3. 检查相同task_id的分指机构记录数量（可根据实际业务调整上限）
            int sameTaskCount = await db.Splits
                .Where(s => s.task_id == addSplitDto.task_id)
                .CountAsync();

            // 假设每个任务最多允许添加的分指机构数量，可根据实际业务修改
            const int maxSplitsPerTask = 5;
            if (sameTaskCount >= maxSplitsPerTask)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addSplitDto.task_id}' 的分指机构数量已达到上限（{maxSplitsPerTask}个），无法继续添加";
                _logger.LogWarning("新增分指机构失败：任务分指机构数量已达上限 - TaskId: {TaskId}, 当前数量: {Count}",
                    addSplitDto.task_id, sameTaskCount);
                return BadRequest(response);
            }

            // 4. 检查生产任务是否存在
            bool taskExists = await db.ProductTasks.AnyAsync(t => t.task_id == addSplitDto.task_id);
            if (!taskExists)
            {
                response.ResultCode = -1;
                response.Msg = $"生产单号 '{addSplitDto.task_id}' 不存在，请先创建该生产任务";
                _logger.LogWarning("新增分指机构失败：生产任务不存在 - TaskId: {TaskId}", addSplitDto.task_id);
                return BadRequest(response);
            }

            // 5. 检查操作员是否存在
            bool operatorExists = await db.Employees.AnyAsync(e => e.employee_id == addSplitDto.operator_id);
            if (!operatorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"操作员ID '{addSplitDto.operator_id}' 不存在，请检查操作员信息";
                _logger.LogWarning("新增分指机构失败：操作员不存在 - OperatorId: {OperatorId}", addSplitDto.operator_id);
                return BadRequest(response);
            }

            // 6. 使用 AutoMapper 将 DTO 转换为 Model
            var splitModel = mapper.Map<SplitModel>(addSplitDto);
            splitModel.updated_at = null;
            // 7. 补充DTO中未包含但Model需要的字段
            splitModel.palm_id = null; // 新增时未绑定手掌
            splitModel.created_at = DateTime.Now; // 设置创建时间

            // 8. 添加到数据库
            await db.Splits.AddAsync(splitModel);
            await db.SaveChangesAsync();

            // 9. 记录日志
            _logger.LogInformation("成功新增分指机构，分指机构ID: {SplitId}, 任务ID: {TaskId}, 当前任务分指机构数量: {Count}",
                addSplitDto.split_id, addSplitDto.task_id, sameTaskCount + 1);

            // 10. 构建响应
            response.ResultCode = 1;
            response.Msg = "新增成功";
            response.ResultData = new
            {
                split = mapper.Map<SplitDto>(splitModel),
                current_count = sameTaskCount + 1,
                max_allowed = maxSplitsPerTask,
                remaining_slots = maxSplitsPerTask - (sameTaskCount + 1)
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            if (dbEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                response.ResultCode = -1;
                response.Msg = $"分指机构ID '{addSplitDto.split_id}' 已存在（数据库约束）";
                _logger.LogWarning("新增分指机构失败：数据库唯一约束违反 - {SplitId}", addSplitDto.split_id);
            }
            else
            {
                response.ResultCode = -1;
                response.Msg = "数据库操作失败";
                _logger.LogError(dbEx, "新增分指机构时数据库操作失败，分指机构ID: {SplitId}", addSplitDto?.split_id);
            }

            return BadRequest(response);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "新增失败";

            _logger.LogError(e, "新增分指机构时发生错误，分指机构ID: {SplitId}", addSplitDto?.split_id);

            return StatusCode(500, response);
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse>> SplitBindPalm(SplitBindPalmDto dto)
    {
        var res = new ApiResponse();

        try
        {
            // 查询分指机构是否存在（原查询手指的逻辑改为查询分指机构）
            var split = await db.Splits
                .FirstOrDefaultAsync(t => t.split_id == dto.split_id);
        
            // // 查询手掌是否存在（保持手掌查询逻辑不变）
            // var palm = await db.Materials
            //     .FirstOrDefaultAsync(t => t.material_id == dto.palm_id);

            // 分指机构不存在的校验（原手指不存在提示改为分指机构）
            if (split == null)
            {
                res.ResultCode = -1;
                res.Msg = "分指机构不存在";
                return NotFound(res);
            }
        
            // // 手掌不存在的校验（保持不变）
            // if (palm == null)
            // {
            //     res.ResultCode = -1;
            //     res.Msg = "手掌外壳不存在";
            //     return NotFound(res);
            // }

            // 绑定逻辑：将分指机构与手掌关联（原手指绑定改为分指机构绑定）
            split.palm_id = dto.palm_id;
            split.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            // 返回结果：更新为分指机构的绑定结果
            res.ResultCode = 1;
            res.Msg = "分指机构绑定手掌成功";
            res.ResultData = split;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"分指机构绑定失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateQualify(UpdateQualifyDto qualifyDto)
    {
        var res = new ApiResponse();

        try
        {
            var split = await db.Splits
                .FirstOrDefaultAsync(t => t.split_id == qualifyDto.id);

            if (split == null)
            {
                res.ResultCode = -1;
                res.Msg = "分指机构不存在";
                return NotFound(res);
            }


            split.is_qualified = qualifyDto.qualified;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "更新成功";
            res.ResultData = split;

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
    public async Task<ActionResult<ApiResponse>> UnBindSplit(string split_id)

    {
        var res = new ApiResponse();

        try
        {
            var split = await db.Splits
                .FirstOrDefaultAsync(t => t.split_id == split_id);

            if (split == null)
            {
                res.ResultCode = -1;
                res.Msg = "分指机构不存在";
                return NotFound(res);
            }

            split.task_id = "";
            split.palm_id= "";
            split.updated_at = null;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "分指机构解绑成功";
            res.ResultData = split;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"分指机构解绑失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> ReBindSplit(ReBindDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var split = await db.Splits
                .FirstOrDefaultAsync(t => t.split_id == dto.part_id);

            if (split == null)
            {
                res.ResultCode = -1;
                res.Msg = "分指机构不存在";
                return NotFound(res);
            }

            split.task_id = dto.task_id;
            split.palm_id = dto.on_part_id;
            split.updated_at = DateTime.Now;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "分指机构更新成功";
            res.ResultData = split;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"分指机构更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse>> UpdateSplit(SplitDto dto)
    {
        var res = new ApiResponse();

        try
        {
            var split = await db.Splits
                .FirstOrDefaultAsync(t => t.split_id == dto.split_id);

            if (split == null)
            {
                res.ResultCode = -1;
                res.Msg = "分指机构不存在";
                return NotFound(res);
            }

            split.task_id = dto.task_id;
            split.operator_id = dto.operator_id;
            split.remarks = dto.remarks;
            split.is_qualified = dto.is_qualified;
            split.palm_id = dto.palm_id;
            split.updated_at = dto.updated_at;

            await db.SaveChangesAsync();

            res.ResultCode = 1;
            res.Msg = "分指机构更新成功";
            res.ResultData = split;

            return Ok(res);
        }
        catch (Exception ex)
        {
            res.ResultCode = -1;
            res.Msg = $"分指机构更新失败: {ex.Message}";
            return BadRequest(res);
        }
    }
}