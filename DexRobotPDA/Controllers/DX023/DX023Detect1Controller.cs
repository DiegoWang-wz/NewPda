using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using DexRobotPDA.Services;
using Microsoft.AspNetCore.Mvc;

namespace DexRobotPDA.Controllers.DX023;

[ApiController]
[Route("api/[controller]/[action]")]
public class DX023Detect1Controller : ControllerBase
{
    private readonly IDX023Service _idx023Service;

    public DX023Detect1Controller(IDX023Service idx023Service)
    {
        _idx023Service = idx023Service;
    }

    [HttpPost]
    public Task<ApiResponse<bool>> AddDX023CalibrateDetect(AddDX023CalibrateDetectsDto dto,
        CancellationToken ct = default)
        => _idx023Service.AddDX023CalibrateDetect(dto, ct);

    [HttpPost]
    public Task<ApiResponse<List<DX023CalibrateDetectsDto>>> GetDX023CalibrateDetectByPalm(string palm_id,
        CancellationToken ct = default) => _idx023Service.GetDX023CalibrateDetectByPalm(palm_id, ct);
}