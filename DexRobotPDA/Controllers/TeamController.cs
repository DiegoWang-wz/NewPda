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
public class TeamController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<TeamController> _logger;
    public TeamController(DailyDbContext _db, IMapper _mapper,ILogger<TeamController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult GetTeams()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Teams.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);
            
            List<TeamDto> Teams = mapper.Map<List<TeamDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Teams;
            
            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Teams.Count);
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