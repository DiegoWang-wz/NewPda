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
public class EventLogController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<EventLogController> _logger;
    public EventLogController(DailyDbContext _db, IMapper _mapper,ILogger<EventLogController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult GetEventLogs()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.EventLogs.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);
            
            List<EventLogDto> Logs = mapper.Map<List<EventLogDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Logs;
            
            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Logs.Count);
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
    public async Task<IActionResult> AddEventLogs(AddEventLogDto dto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var model = mapper.Map<EventLogModel>(dto);
            await db.EventLogs.AddAsync(model);
            await db.SaveChangesAsync();
            _logger.LogInformation("成功新增日志，日志ID: {DetectId}",model.id);
            response.ResultCode = 1;
            response.Msg = "新增日志成功";
            return Ok(response);
        }
        catch (Exception ex)
        {
            // 处理其他未知异常
            response.ResultCode = -1;
            response.Msg = "新增检测记录失败";
            _logger.LogError(ex, "新增检测记录时发生错误");

            return StatusCode(500, response);
        }
    }
    
    [HttpGet]
    public IActionResult GetEventLogsByDateRange([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            IQueryable<EventLogModel> query = db.EventLogs.AsQueryable();

            // 如果提供了开始日期，则筛选大于等于开始日期的记录
            if (startDate.HasValue)
            {
                query = query.Where(e => e.operate_time >= startDate.Value);
            }

            // 如果提供了结束日期，则筛选小于等于结束日期的记录
            if (endDate.HasValue)
            {
                // 通常将结束日期设为当天的最后一秒
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.operate_time <= endOfDay);
            }

            // 按创建时间降序排列
            var list = query.OrderByDescending(e => e.operate_time).ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<EventLogDto> logs = mapper.Map<List<EventLogDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = logs;

            // 记录成功信息
            _logger.LogInformation("成功获取日期范围内的日志，共{Count}条记录", logs.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "Error";

            // 记录错误信息，包括异常详情
            _logger.LogError(e, "根据日期范围获取日志时发生错误");
        }

        return Ok(response);
    }

 }