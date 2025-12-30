using AutoMapper;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using Microsoft.AspNetCore.Mvc;
using DexRobotPDA.Services;

namespace DexRobotPDA.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CodeIdentifyController : ControllerBase
    {
        
        private readonly DailyDbContext db;
        private readonly IMapper mapper;
        private readonly ILogger<ProductTaskController> _logger;

        public CodeIdentifyController(DailyDbContext _db, IMapper _mapper, ILogger<ProductTaskController> logger)
        {
            db = _db;
            mapper = _mapper;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<SimplePartInfo> Identify([FromQuery] string code)
        {
            var info = PartCodeHelper.Parse(code);
            return Ok(info);
        }

        [HttpPost]
        public ActionResult<SimplePartInfo> IdentifyPost([FromBody] string code)
        {
            var info = PartCodeHelper.Parse(code);
            return Ok(info);
        }
        
        [HttpGet]
        public async Task<IActionResult> Trace([FromQuery] string code, CancellationToken ct = default)
        {
            var resp = new ApiResponse<TracePathDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    resp.ResultCode = -1;
                    resp.Msg = "code 不能为空";
                    resp.ResultData = null;
                    return Ok(resp);
                }

                var result = await PartCodeHelper.TraceAsync(db, code.Trim(), ct);

                resp.ResultCode = 1;
                resp.Msg = "Success";
                resp.ResultData = result;
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trace failed, code={Code}", code);

                resp.ResultCode = -1;
                resp.Msg = "Error";
                resp.ResultData = null;
                return Ok(resp);
            }
        }

    }
}