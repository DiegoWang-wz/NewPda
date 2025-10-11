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
    
    [HttpPost]
    public async Task<IActionResult> AddEventLog(AddEventLogDto dto)
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
    public IActionResult GetEventLogs([FromQuery] string startDate, [FromQuery] string endDate)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
            {
                response.ResultCode = -1;
                response.Msg = "日期格式错误，应为 yyyy-MM-dd";
                return BadRequest(response);
            }

            // 确保结束日期包含当日的记录
            end = end.AddDays(1);

            // 查询在指定日期范围内的记录
            var list = db.EventLogs
                .Where(e => e.operate_time >= start && e.operate_time < end)
                .OrderByDescending(e => e.operate_time)
                .ToList();

            _logger.LogDebug("在 {Start} 至 {End} 获取到 {Count} 条记录", start, end, list.Count);

            List<EventLogDto> Logs = mapper.Map<List<EventLogDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Logs;

            _logger.LogInformation("成功根据日期范围获取，共 {Count} 条记录", Logs.Count);
        }
        catch (Exception ex)
        {
            response.ResultCode = -1;
            response.Msg = "Error";
            _logger.LogError(ex, "根据日期范围获取记录时发生错误");
        }

        return Ok(response);
    }

 }