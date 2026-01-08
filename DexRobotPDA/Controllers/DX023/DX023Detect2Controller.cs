using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using DexRobotPDA.Services;
using Microsoft.AspNetCore.Mvc;

namespace DexRobotPDA.Controllers.DX023;

[ApiController]
[Route("api/[controller]/[action]")]
public class DX023Detect2Controller : ControllerBase
{
    private readonly IDX023Service _idx023Service;

    public DX023Detect2Controller(IDX023Service idx023Service)
    {
        _idx023Service = idx023Service;
    }

    [HttpPost]
    public Task<ApiResponse<bool>> AddDX023FunctionalDetect(AddDX023FunctionalDetectsDto dto,
        CancellationToken ct = default)
        => _idx023Service.AddDX023FunctionalDetect(dto, ct);

    [HttpGet]
    public Task<ApiResponse<List<DX023FunctionalDetectsDto>>> GetDX023FunctionalDetectByPalm(string palm_id,
        CancellationToken ct = default) => _idx023Service.GetDX023FunctionalDetectByPalm(palm_id, ct);

    [HttpGet]
    public Task<ApiResponse<bool>> GetProcess1Status(string task_id, CancellationToken ct = default) =>
        _idx023Service.GetProcess1Status(task_id, ct);

    [HttpGet]
    public Task<ApiResponse<bool>> GetProcess2Status(string task_id, CancellationToken ct = default) =>
        _idx023Service.GetProcess2Status(task_id, ct);

    [HttpGet]
    public Task<ApiResponse<bool>> GetProcess3Status(string task_id, CancellationToken ct = default) =>
        _idx023Service.GetProcess3Status(task_id, ct);
    
    [HttpGet]
    public Task<ApiResponse<bool>> GetProcess4Status(string task_id, CancellationToken ct = default) =>
        _idx023Service.GetProcess4Status(task_id, ct);
    
    [HttpGet]
    public Task<ApiResponse<bool>> GetProcess5Status(string task_id, CancellationToken ct = default) =>
        _idx023Service.GetProcess5Status(task_id, ct);
    
}