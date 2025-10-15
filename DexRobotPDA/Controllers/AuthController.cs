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
public class AuthController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<AuthController> _logger;
    public AuthController(DailyDbContext _db, IMapper _mapper,ILogger<AuthController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }
    
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        string employee_id = loginRequest.employee_id;
        string password = loginRequest.password; 
        ApiResponse response = new ApiResponse();
        try
        {
            
            // 根据员工ID查询数据库
            var employee = db.Employees.FirstOrDefault(e => e.employee_id == employee_id);
            _logger.LogDebug("尝试登录 - 员工ID: {EmployeeId}", employee_id);

            if (employee == null)
            {
                // 员工不存在
                response.ResultCode = 0;
                response.Msg = "员工ID不存在";
                _logger.LogWarning("登录失败 - 员工ID {EmployeeId} 不存在", employee_id);
            }
            else if (employee.status != 1)
            {
                // 员工状态非在职
                response.ResultCode = 0;
                response.Msg = "该员工已离职或状态异常，无法登录";
                _logger.LogWarning("登录失败 - 员工ID {EmployeeId} 状态异常（状态码: {Status}", 
                    employee_id, employee.status);
            }
            else
            {
                if (employee.password == password)
                {
                    var userDto = mapper.Map<UserDto>(employee);
                    response.ResultCode = 1;
                    response.Msg = "登录成功";
                    response.ResultData = userDto;
                    _logger.LogInformation("登录成功 - 员工ID: {EmployeeId}", employee_id);
                }
                else
                {
                    // 密码错误
                    response.ResultCode = 0;
                    response.Msg = "密码错误";
                    _logger.LogWarning("登录失败 - 员工ID {EmployeeId} 密码错误", employee_id);
                }
            }
        }
        catch (Exception ex)
        {
            response.ResultCode = -1;
            response.Msg = "登录过程发生错误";
            _logger.LogError(ex, "登录异常 - 员工ID: {EmployeeId}", employee_id);
        }

        return Ok(response);
    }
 }