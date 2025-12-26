using AutoMapper;
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

        // POST /api/codeidentify
        // Body: "MO-20250411-001-FL-0001"
        [HttpPost]
        public ActionResult<SimplePartInfo> IdentifyPost([FromBody] string code)
        {
            var info = PartCodeHelper.Parse(code);
            return Ok(info);
        }
        
        [HttpGet("trace")]
        public async Task<ActionResult<TraceChainDto>> Trace([FromQuery] string code, CancellationToken ct)
        {
            var res = await PartCodeHelper.TraceAsync(db, code, ct);
            return Ok(res);
        }

    }
}