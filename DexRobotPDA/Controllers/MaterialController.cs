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
public class MaterialController : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<MaterialController> _logger;
    public MaterialController(DailyDbContext _db, IMapper _mapper,ILogger<MaterialController> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult GetMaterials()
    {
        ApiResponse response = new ApiResponse();
        try
        {
            var list = db.Materials.ToList();
            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);
            
            List<MaterialDto> materials = mapper.Map<List<MaterialDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = materials;
            
            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", materials.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "Error";
            
            _logger.LogError(e, "获取列表时发生错误");
        }

        return Ok(response);
    }
 }