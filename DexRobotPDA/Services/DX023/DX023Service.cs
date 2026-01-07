using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;
using DexRobotPDA.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DexRobotPDA.Services
{
    public interface IDX023Service
    {
        Task<ApiResponse<bool>>
            AddDX023CalibrateDetect(AddDX023CalibrateDetectsDto dto, CancellationToken ct = default);

        Task<ApiResponse<List<DX023CalibrateDetectsDto>>> GetDX023CalibrateDetectByPalm(string palm_id,
            CancellationToken ct = default);

        Task<ApiResponse<bool>> AddDX023FunctionalDetect(AddDX023FunctionalDetectsDto dto,
            CancellationToken ct = default);

        Task<ApiResponse<List<DX023FunctionalDetectsDto>>> GetDX023FunctionalDetectByPalm(string palm_id,
            CancellationToken ct = default);

        Task<ApiResponse<bool>> GetProcess1Status(string task_id, CancellationToken ct = default);
        Task<ApiResponse<bool>> GetProcess2Status(string task_id, CancellationToken ct = default);
        Task<ApiResponse<bool>> GetProcess3Status(string task_id, CancellationToken ct = default);
        Task<ApiResponse<bool>> GetProcess4Status(string task_id, CancellationToken ct = default);
    }

    public class DX023Service : IDX023Service
    {
        private readonly DailyDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<DX023Service> _logger;
        private readonly IPartService _partService;

        // ✅ 必须有构造函数注入并赋值
        public DX023Service(
            DailyDbContext db,
            IMapper mapper,
            ILogger<DX023Service> logger,
            IPartService partService // ✅ 新增
        )
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _partService = partService ?? throw new ArgumentNullException(nameof(partService));
        }

        public async Task<ApiResponse<bool>> AddDX023CalibrateDetect(AddDX023CalibrateDetectsDto dto,
            CancellationToken ct = default)
        {
            try
            {
                if (dto == null) return ApiResponse<bool>.BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.palm_id))
                    return ApiResponse<bool>.BadRequest("手掌id不能为空");

                if (string.IsNullOrWhiteSpace(dto.inspector))
                    return ApiResponse<bool>.BadRequest("操作人员id不能为空");

                var info = PartCodeHelper.Parse(dto.palm_id);
                if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<bool>.Fail("palm_id编码错误", data: false);

                var entity = _mapper.Map<DX023CalibrateDetectsModel>(dto);

                if (entity.calibrate_time == default)
                    entity.calibrate_time = DateTime.Now;

                _db.DX023CalibrateDetects.Add(entity);
                var rows = await _db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "AddDX023CalibrateDetect success, inserted_rows={Rows}, new_id={Id}, palm_id={PalmId}, inspector={Inspector}",
                    rows, entity.id, dto.palm_id, dto.inspector
                );

                return rows > 0
                    ? ApiResponse<bool>.Ok(true, "OK")
                    : ApiResponse<bool>.Fail("保存失败", data: false);
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.Root();
                _logger.LogError(dbEx, "AddDX023CalibrateDetect DbUpdateException, palm_id={PalmId}, root={Root}",
                    dto?.palm_id, root.Message);

                return ApiResponse<bool>.Fail($"保存失败：{root.Message}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("AddDX023CalibrateDetect canceled, palm_id={PalmId}", dto?.palm_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "AddDX023CalibrateDetect failed, palm_id={PalmId}, root={Root}",
                    dto?.palm_id, root.Message);

                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        public async Task<ApiResponse<List<DX023CalibrateDetectsDto>>> GetDX023CalibrateDetectByPalm(string palm_id,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(palm_id))
                {
                    _logger.LogWarning("Query GetDX023CalibrateDetectByTaskId: palm_id is empty");
                    return ApiResponse<List<DX023CalibrateDetectsDto>>.BadRequest("palm_id不能为空");
                }

                var list = await _db.DX023CalibrateDetects
                    .AsNoTracking()
                    .Where(x => x.palm_id == palm_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<DX023CalibrateDetectsDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query DX023CalibrateDetect by palm_id done, palm_id={palm_id}, count={Count}",
                    palm_id,
                    list.Count);

                return ApiResponse<List<DX023CalibrateDetectsDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetDX023CalibrateDetectByTaskId canceled, palm_id={palm_id}", palm_id);
                return ApiResponse<List<DX023CalibrateDetectsDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetDX023CalibrateDetectByTaskId failed, palm_id={palm_id}, root={Root}", palm_id,
                    root.Message);
                return ApiResponse<List<DX023CalibrateDetectsDto>>.Fail($"系统异常：{root.Message}",
                    data: new List<DX023CalibrateDetectsDto>());
            }
        }

        public async Task<ApiResponse<bool>> AddDX023FunctionalDetect(AddDX023FunctionalDetectsDto dto,
            CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                if (dto == null) return ApiResponse<bool>.BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.palm_id))
                    return ApiResponse<bool>.BadRequest("手掌id不能为空");

                if (string.IsNullOrWhiteSpace(dto.inspector))
                    return ApiResponse<bool>.BadRequest("操作人员id不能为空");

                var info = PartCodeHelper.Parse(dto.palm_id);
                if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<bool>.Fail("palm_id编码错误", data: false);

                var entity = _mapper.Map<DX023FunctionalDetectsModel>(dto);

                if (entity.calibrate_time == default)
                    entity.calibrate_time = DateTime.Now;

                _db.DX023FunctionalDetects.Add(entity);
                var rows = await _db.SaveChangesAsync(ct);

                if (rows <= 0)
                {
                    await tx.RollbackAsync(ct);
                    return ApiResponse<bool>.Fail("保存失败", data: false);
                }

                _logger.LogInformation(
                    "AddDX023FunctionalDetect success, inserted_rows={Rows}, new_id={Id}, palm_id={PalmId}, inspector={Inspector}",
                    rows, entity.id, dto.palm_id, dto.inspector
                );

                var newdto = new UpdatePalmDto
                {
                    palm_id = dto.palm_id,
                    is_qualified = dto.if_qualified
                };

                var res = await _partService.UpdatePalm(newdto, ct);

                if (res.ResultCode != 1)
                {
                    await tx.RollbackAsync(ct);
                    return ApiResponse<bool>.Fail($"新增检测成功，但更新Palm失败：{res.Msg}", data: false);
                }

                await tx.CommitAsync(ct);
                return ApiResponse<bool>.Ok(true, "OK");
            }
            catch (DbUpdateException dbEx)
            {
                try
                {
                    await tx.RollbackAsync(ct);
                }
                catch
                {
                }

                var root = dbEx.Root();
                _logger.LogError(dbEx, "AddDX023FunctionalDetect DbUpdateException, palm_id={PalmId}, root={Root}",
                    dto?.palm_id, root.Message);

                return ApiResponse<bool>.Fail($"保存失败：{root.Message}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                try
                {
                    await tx.RollbackAsync(ct);
                }
                catch
                {
                }

                _logger.LogWarning("AddDX023FunctionalDetect canceled, palm_id={PalmId}", dto?.palm_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                try
                {
                    await tx.RollbackAsync(ct);
                }
                catch
                {
                }

                var root = ex.Root();
                _logger.LogError(ex, "AddDX023FunctionalDetect failed, palm_id={PalmId}, root={Root}",
                    dto?.palm_id, root.Message);

                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        public async Task<ApiResponse<List<DX023FunctionalDetectsDto>>> GetDX023FunctionalDetectByPalm(string palm_id,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(palm_id))
                {
                    _logger.LogWarning("Query GetDX023FunctionalDetectByTaskId: palm_id is empty");
                    return ApiResponse<List<DX023FunctionalDetectsDto>>.BadRequest("palm_id不能为空");
                }

                var list = await _db.DX023FunctionalDetects
                    .AsNoTracking()
                    .Where(x => x.palm_id == palm_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<DX023FunctionalDetectsDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query DX023FunctionalDetect by palm_id done, palm_id={palm_id}, count={Count}",
                    palm_id,
                    list.Count);

                return ApiResponse<List<DX023FunctionalDetectsDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetDX023FunctionalDetectByTaskId canceled, palm_id={palm_id}", palm_id);
                return ApiResponse<List<DX023FunctionalDetectsDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetDX023FunctionalDetectByTaskId failed, palm_id={palm_id}, root={Root}", palm_id,
                    root.Message);
                return ApiResponse<List<DX023FunctionalDetectsDto>>.Fail($"系统异常：{root.Message}",
                    data: new List<DX023FunctionalDetectsDto>());
            }
        }

        public async Task<ApiResponse<bool>> GetProcess1Status(string task_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("GetProcess1Status: task_id is empty");
                    return ApiResponse<bool>.BadRequest("task_id不能为空");
                }

                var task = await _db.ProductTasks
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .ProjectTo<ProductTaskDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                int servo_number = task.Count * 5;
                
                
                var list = await _db.Servos
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id && x.type == 1)
                    .ProjectTo<ServoDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                int bind_servo_number = list.Count;


                _logger.LogInformation("GetProcess1Status, 应绑定数量={servo_number}, 实际绑定数量={bind_servo_number}",
                    servo_number,
                    bind_servo_number);

                return (bind_servo_number == servo_number)
                    ? ApiResponse<bool>.Ok(true, "OK")
                    : ApiResponse<bool>.Fail($"绑定数量不满足,应绑定数量={servo_number}, 实际绑定数量={bind_servo_number}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetProcess1Status canceled, task_id={task_id}", task_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetProcess1Status failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }
        
        public async Task<ApiResponse<bool>> GetProcess2Status(string task_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("GetProcess2Status: task_id is empty");
                    return ApiResponse<bool>.BadRequest("task_id不能为空");
                }

                var task = await _db.ProductTasks
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .ProjectTo<ProductTaskDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                int finger_number = task.Count * 3;
                
                var list = await _db.Fingers
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id && (x.type == 4 || x.type == 3))
                    .ProjectTo<FingerDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                
                int bind_Finger_number = list.Count;
                
                _logger.LogInformation("GetProcess2Status, 应绑定数量={finger_number}, 实际绑定数量={bind_Finger_number}",
                    finger_number,
                    bind_Finger_number);

                return (finger_number == bind_Finger_number)
                    ? ApiResponse<bool>.Ok(true, "OK")
                    : ApiResponse<bool>.Fail($"绑定数量不满足,应绑定数量={finger_number}, 实际绑定数量={bind_Finger_number}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetProcess2Status canceled, task_id={task_id}", task_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetProcess2Status failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }
        
        public async Task<ApiResponse<bool>> GetProcess3Status(string task_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("GetProcess3Status: task_id is empty");
                    return ApiResponse<bool>.BadRequest("task_id不能为空");
                }

                var task = await _db.ProductTasks
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .ProjectTo<ProductTaskDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                
                
                var list = await _db.Palms
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .ProjectTo<PalmDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                
                int bind_palm_number = list.Count;


                _logger.LogInformation("GetProcess3Status, 应绑定数量={task.Count}, 实际绑定数量={bind_palm_number}",
                    task.Count,
                    bind_palm_number);

                return (task.Count == bind_palm_number)
                    ? ApiResponse<bool>.Ok(true, "OK")
                    : ApiResponse<bool>.Fail($"绑定数量不满足,应绑定数量={task.Count}, 实际绑定数量={bind_palm_number}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetProcess3Status canceled, task_id={task_id}", task_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetProcess3Status failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        public async Task<ApiResponse<bool>> GetProcess4Status(string task_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("GetProcess4Status: task_id is empty");
                    return ApiResponse<bool>.BadRequest("task_id不能为空");
                }

                var task = await _db.ProductTasks
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .ProjectTo<ProductTaskDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                
                // var palms = await _db.Palms
                //     .AsNoTracking()
                //     .Where(x => x.task_id == task_id)
                //     .ProjectTo<PalmDto>(_mapper.ConfigurationProvider)
                //     .ToListAsync(ct);
                //
                // var list = await _db.DX023CalibrateDetects
                //     .AsNoTracking()
                //     .Where(x => x.palm_id == palm_id)
                //     .ProjectTo<PalmDto>(_mapper.ConfigurationProvider)
                //     .ToListAsync(ct);
                
                var qualifiedCount = await (
                    from p in _db.Palms.AsNoTracking()
                    where p.task_id == task_id

                    // 对每个 palm 做子查询：只取最新 1 条（Left Join 语义）
                    from d in _db.DX023CalibrateDetects.AsNoTracking()
                        .Where(x => x.palm_id == p.palm_id)
                        .OrderByDescending(x => x.id) // ✅ 最新规则：id 最大
                        .Take(1)
                        .DefaultIfEmpty()

                    select d
                ).CountAsync(d => d != null && d.if_qualified == true, ct);
                _logger.LogInformation("GetProcess4Status, 应绑定数量={task.Count}, 实际绑定数量={bind_palm_number}",
                    task.Count,
                    qualifiedCount);

                return (task.Count == qualifiedCount)
                    ? ApiResponse<bool>.Ok(true, "OK")
                    : ApiResponse<bool>.Fail($"绑定数量不满足,应绑定数量={task.Count}, 实际绑定数量={qualifiedCount}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetProcess4Status canceled, task_id={task_id}", task_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetProcess4Status failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }
        
        public async Task<ApiResponse<bool>> GetProcess5Status(string task_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("GetProcess5Status: task_id is empty");
                    return ApiResponse<bool>.BadRequest("task_id不能为空");
                }

                var task = await _db.ProductTasks
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .ProjectTo<ProductTaskDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);
                
                var qualifiedCount = await (
                    from p in _db.Palms.AsNoTracking()
                    where p.task_id == task_id

                    from d in _db.DX023FunctionalDetects.AsNoTracking()
                        .Where(x => x.palm_id == p.palm_id)
                        .OrderByDescending(x => x.id) // ✅ 最新规则：id 最大
                        .Take(1)
                        .DefaultIfEmpty()

                    select d
                ).CountAsync(d => d != null && d.if_qualified == true, ct);
                _logger.LogInformation("GetProcess5Status, 应绑定数量={task.Count}, 实际绑定数量={bind_palm_number}",
                    task.Count,
                    qualifiedCount);

                return (task.Count == qualifiedCount)
                    ? ApiResponse<bool>.Ok(true, "OK")
                    : ApiResponse<bool>.Fail($"绑定数量不满足,应绑定数量={task.Count}, 实际绑定数量={qualifiedCount}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetProcess5Status canceled, task_id={task_id}", task_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetProcess5Status failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }
    }
}