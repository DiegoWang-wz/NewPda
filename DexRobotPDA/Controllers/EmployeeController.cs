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
public class EmployeeController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<EmployeeController> _logger;
    public EmployeeController(DailyDbContext _db, IMapper _mapper,ILogger<EmployeeController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult GetEmployee()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Employees.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);
            
            List<EmployeeDto> employees = mapper.Map<List<EmployeeDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = employees;
            
            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", employees.Count);
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