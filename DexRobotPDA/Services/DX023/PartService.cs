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
    public interface IPartService
    {
        #region Motor

        #endregion

        #region Servo

        Task<ApiResponse<List<ServoDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ApiResponse<ServoDto?>> GetServoByIdAsync(string servo_id, CancellationToken ct = default);
        Task<ApiResponse<List<ServoDto>>> GetServoByTaskIdAsync(string task_id, CancellationToken ct = default);
        Task<ApiResponse<bool>> UnbindServoAsync(string servo_id, CancellationToken ct = default);
        Task<ApiResponse<bool>> RebindServoAsync(RebindServoDto dto, CancellationToken ct = default);
        Task<ApiResponse<bool>> AddServoAsync(AddServoDto dto, CancellationToken ct = default);
        Task<ApiResponse<bool>> ServoBindFingerAsync(ServoBindFingerDto dto, CancellationToken ct = default);
        Task<ApiResponse<List<ServoDto>>> GetFingerDetailAsync(string superior_id, CancellationToken ct = default);

        #endregion

        #region Finger

        Task<ApiResponse<List<FingerDto>>> GetAllFingerAsync(CancellationToken ct = default);

        Task<ApiResponse<List<FingerDto>>> GetFingerByTaskId(string task_id,
            CancellationToken ct = default);

        #endregion

        #region Palm

        Task<ApiResponse<List<PalmDto>>> GetPalmByTaskId(string task_id,
            CancellationToken ct = default);

        Task<ApiResponse<bool>> UpdatePalm(UpdatePalmDto dto,
            CancellationToken ct = default);

        #endregion
    }

    public class PartService : IPartService
    {
        private readonly DailyDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<PartService> _logger;

        public PartService(DailyDbContext db, IMapper mapper, ILogger<PartService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        #region Motor

        #endregion

        #region Servo

        public async Task<ApiResponse<List<ServoDto>>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _db.Servos
                    .AsNoTracking()
                    .OrderByDescending(x => x.id)
                    .ProjectTo<ServoDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Servos done, count={Count}", list.Count);
                return ApiResponse<List<ServoDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetAllAsync canceled");
                return ApiResponse<List<ServoDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetAllAsync failed, root={Root}", root.Message);
                return ApiResponse<List<ServoDto>>.Fail($"系统异常：{root.Message}", data: new List<ServoDto>());
            }
        }

        public async Task<ApiResponse<ServoDto?>> GetServoByIdAsync(string servo_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(servo_id))
                {
                    _logger.LogWarning("GetServoByIdAsync: servo_id is empty");
                    return ApiResponse<ServoDto?>.BadRequest("servo_id不能为空");
                }

                var entity = await _db.Servos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Servo_id == servo_id, ct);

                if (entity == null)
                {
                    _logger.LogWarning("Query Servo not found, servo_id={ServoId}", servo_id);
                    return ApiResponse<ServoDto?>.NotFound("Not Found");
                }

                var dto = _mapper.Map<ServoDto>(entity);
                return ApiResponse<ServoDto?>.Ok(dto, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetServoByIdAsync canceled, servo_id={ServoId}", servo_id);
                return ApiResponse<ServoDto?>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetServoByIdAsync failed, servo_id={ServoId}, root={Root}", servo_id,
                    root.Message);
                return ApiResponse<ServoDto?>.Fail($"系统异常：{root.Message}");
            }
        }

        public async Task<ApiResponse<List<ServoDto>>> GetServoByTaskIdAsync(string task_id,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("Query Servos by task_id: task_id is empty");
                    return ApiResponse<List<ServoDto>>.BadRequest("task_id不能为空");
                }

                var list = await _db.Servos
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<ServoDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Servos by task_id done, task_id={task_id}, count={Count}", task_id,
                    list.Count);

                return ApiResponse<List<ServoDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetServoByTaskIdAsync canceled, task_id={task_id}", task_id);
                return ApiResponse<List<ServoDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetServoByTaskIdAsync failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<List<ServoDto>>.Fail($"系统异常：{root.Message}", data: new List<ServoDto>());
            }
        }

        public async Task<ApiResponse<List<ServoDto>>> GetFingerDetailAsync(string superior_id,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(superior_id))
                {
                    _logger.LogWarning("GetFingerDetailAsync: superior_id is empty");
                    return ApiResponse<List<ServoDto>>.BadRequest("superior_id不能为空");
                }

                var list = await _db.Servos
                    .AsNoTracking()
                    .Where(x => x.superior_id == superior_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<ServoDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Servos by superior_id done, superior_id={SuperiorId}, count={Count}",
                    superior_id, list.Count);

                return ApiResponse<List<ServoDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetFingerDetailAsync canceled, superior_id={SuperiorId}", superior_id);
                return ApiResponse<List<ServoDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetFingerDetailAsync failed, superior_id={SuperiorId}, root={Root}", superior_id,
                    root.Message);
                return ApiResponse<List<ServoDto>>.Fail($"系统异常：{root.Message}", data: new List<ServoDto>());
            }
        }

        public async Task<ApiResponse<bool>> UnbindServoAsync(string servo_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(servo_id))
                    return ApiResponse<bool>.BadRequest("servo_id不能为空");

                var rows = await _db.Servos
                    .Where(t => t.Servo_id == servo_id)
                    .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.superior_id, (string?)null)
                            .SetProperty(x => x.task_id, (string?)null)
                            .SetProperty(x => x.updated_at, DateTime.Now),
                        ct);

                if (rows == 0)
                {
                    _logger.LogWarning("UnbindServoAsync: Servo not found, servo_id={ServoId}", servo_id);
                    return ApiResponse<bool>.Fail($"Servo不存在：{servo_id}", data: false);
                }

                return ApiResponse<bool>.Ok(true, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("UnbindServoAsync canceled, servo_id={ServoId}", servo_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "UnbindServoAsync failed, servo_id={ServoId}, root={Root}", servo_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        public async Task<ApiResponse<bool>> RebindServoAsync(RebindServoDto dto, CancellationToken ct = default)
        {
            try
            {
                if (dto == null) return ApiResponse<bool>.BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    return ApiResponse<bool>.BadRequest("servo_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.superior_id))
                    return ApiResponse<bool>.BadRequest("superior_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.task_id))
                    return ApiResponse<bool>.BadRequest("task_id不能为空");

                var rows = await _db.Servos
                    .Where(t => t.Servo_id == dto.servo_id)
                    .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.superior_id, dto.superior_id)
                            .SetProperty(x => x.task_id, dto.task_id)
                            .SetProperty(x => x.updated_at, DateTime.Now),
                        ct);

                if (rows == 0)
                {
                    _logger.LogWarning("RebindServoAsync: Servo not found, servo_id={ServoId}", dto.servo_id);
                    return ApiResponse<bool>.Fail($"Servo不存在：{dto.servo_id}", data: false);
                }

                return ApiResponse<bool>.Ok(true, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("RebindServoAsync canceled, servo_id={ServoId}", dto?.servo_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "RebindServoAsync failed, servo_id={ServoId}, root={Root}", dto?.servo_id,
                    root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        public async Task<ApiResponse<bool>> AddServoAsync(AddServoDto dto, CancellationToken ct = default)
        {
            try
            {
                if (dto == null) return ApiResponse<bool>.BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    return ApiResponse<bool>.BadRequest("servo_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.operator_id))
                    return ApiResponse<bool>.BadRequest("operator_id不能为空");

                var info = PartCodeHelper.Parse(dto.servo_id);
                if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<bool>.Fail("servo_id编码错误", data: false);

                dto.type = info.Result switch
                {
                    "Servo" => 1,
                    "RotaryServo" => 2,
                    _ => 0
                };
                if (dto.type == 0)
                    return ApiResponse<bool>.Fail("servo类型不支持", data: false);

                var exists = await _db.Servos.AsNoTracking().AnyAsync(x => x.Servo_id == dto.servo_id, ct);
                if (exists)
                    return ApiResponse<bool>.Fail($"servo_id已存在：{dto.servo_id}", data: false);

                var entity = _mapper.Map<ServoModel>(dto);
                if (entity.created_at == default)
                    entity.created_at = DateTime.Now;

                _db.Servos.Add(entity);
                var rows = await _db.SaveChangesAsync(ct);

                _logger.LogInformation("AddServoAsync success, inserted_rows={Rows}, new_id={Id}", rows, entity.id);

                return rows > 0 ? ApiResponse<bool>.Ok(true, "OK") : ApiResponse<bool>.Fail("保存失败", data: false);
            }
            catch (DbUpdateException dbEx)
            {
                var root = dbEx.Root();
                _logger.LogError(dbEx, "AddServoAsync DbUpdateException, root={Root}", root.Message);
                return ApiResponse<bool>.Fail($"保存失败：{root.Message}", data: false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("AddServoAsync canceled, servo_id={ServoId}", dto?.servo_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "AddServoAsync failed, root={Root}", root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        public async Task<ApiResponse<bool>> ServoBindFingerAsync(ServoBindFingerDto dto,
            CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                if (dto == null) return ApiResponse<bool>.BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.task_id))
                    return ApiResponse<bool>.BadRequest("task_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.operator_id))
                    return ApiResponse<bool>.BadRequest("operator_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.finger_id))
                    return ApiResponse<bool>.BadRequest("finger_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.servo_id1))
                    return ApiResponse<bool>.BadRequest("servo_id1不能为空");

                // finger 编码校验
                {
                    var info = PartCodeHelper.Parse(dto.finger_id);
                    if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                        return ApiResponse<bool>.Fail("finger编码错误", data: false);
                }

                // servo1
                var servo1Info = PartCodeHelper.Parse(dto.servo_id1);
                if (servo1Info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<bool>.Fail("servo_id1编码错误", data: false);

                var servo1Type = servo1Info.Result switch
                {
                    "Servo" => 1,
                    "RotaryServo" => 2,
                    _ => 0
                };
                if (servo1Type == 0) return ApiResponse<bool>.Fail("servo_id1类型不支持", data: false);

                var servo1 = new AddServoDto
                {
                    task_id = dto.task_id,
                    operator_id = dto.operator_id,
                    servo_id = dto.servo_id1,
                    superior_id = dto.finger_id,
                    remarks = dto.remarks,
                    type = servo1Type
                };

                // servo2（可选）
                AddServoDto? servo2 = null;
                if (!string.IsNullOrWhiteSpace(dto.servo_id2))
                {
                    var servo2Info = PartCodeHelper.Parse(dto.servo_id2);
                    if (servo2Info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                        return ApiResponse<bool>.Fail("servo_id2编码错误", data: false);

                    var servo2Type = servo2Info.Result switch
                    {
                        "Servo" => 1,
                        "RotaryServo" => 2,
                        _ => 0
                    };
                    if (servo2Type == 0) return ApiResponse<bool>.Fail("servo_id2类型不支持", data: false);

                    servo2 = new AddServoDto
                    {
                        task_id = dto.task_id,
                        operator_id = dto.operator_id,
                        servo_id = dto.servo_id2,
                        superior_id = dto.finger_id,
                        remarks = dto.remarks,
                        type = servo2Type
                    };
                }

                // finger
                var fingerDto = new AddFingerDto
                {
                    task_id = dto.task_id,
                    operator_id = dto.operator_id,
                    finger_id = dto.finger_id,
                    remarks = dto.remarks,
                    is_qualified = true,
                    type = string.IsNullOrWhiteSpace(dto.servo_id2) ? 3 : 4
                };

                // 主键重复检查
                var fingerExists = await _db.Fingers.AsNoTracking().AnyAsync(x => x.finger_id == dto.finger_id, ct);
                if (fingerExists)
                {
                    await tx.RollbackAsync(ct);
                    return ApiResponse<bool>.Fail($"finger_id已存在：{dto.finger_id}", data: false);
                }

                var servo1Exists = await _db.Servos.AsNoTracking().AnyAsync(x => x.Servo_id == dto.servo_id1, ct);
                if (servo1Exists)
                {
                    await tx.RollbackAsync(ct);
                    return ApiResponse<bool>.Fail($"servo_id1已存在：{dto.servo_id1}", data: false);
                }

                if (servo2 != null)
                {
                    var servo2Exists = await _db.Servos.AsNoTracking().AnyAsync(x => x.Servo_id == dto.servo_id2, ct);
                    if (servo2Exists)
                    {
                        await tx.RollbackAsync(ct);
                        return ApiResponse<bool>.Fail($"servo_id2已存在：{dto.servo_id2}", data: false);
                    }
                }

                // 入库（同一事务）
                var entityFinger = _mapper.Map<FingerModel>(fingerDto);
                var entityServo1 = _mapper.Map<ServoModel>(servo1);

                _db.Fingers.Add(entityFinger);
                _db.Servos.Add(entityServo1);

                if (servo2 != null)
                {
                    var entityServo2 = _mapper.Map<ServoModel>(servo2);
                    _db.Servos.Add(entityServo2);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation(
                    "ServoBindFingerAsync success, task_id={TaskId}, operator_id={OperatorId}, finger_id={FingerId}, servo1={Servo1}, servo2={Servo2}",
                    dto.task_id, dto.operator_id, dto.finger_id, dto.servo_id1, dto.servo_id2
                );

                return ApiResponse<bool>.Ok(true, "绑定成功");
            }
            catch (DbUpdateException dbEx)
            {
                try
                {
                    await tx.RollbackAsync(ct);
                }
                catch
                {
                    /* ignore */
                }

                var root = dbEx.Root();
                _logger.LogError(dbEx,
                    "ServoBindFingerAsync DbUpdateException rolled back. root={RootMessage}, task_id={TaskId}, finger_id={FingerId}, servo1={Servo1}, servo2={Servo2}",
                    root.Message, dto?.task_id, dto?.finger_id, dto?.servo_id1, dto?.servo_id2);

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
                    /* ignore */
                }

                _logger.LogWarning("ServoBindFingerAsync canceled, task_id={TaskId}, finger_id={FingerId}",
                    dto?.task_id, dto?.finger_id);

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
                    /* ignore */
                }

                var root = ex.Root();
                _logger.LogError(ex,
                    "ServoBindFingerAsync failed and rolled back. root={RootMessage}, task_id={TaskId}, finger_id={FingerId}, servo1={Servo1}, servo2={Servo2}",
                    root.Message, dto?.task_id, dto?.finger_id, dto?.servo_id1, dto?.servo_id2);

                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        #endregion

        #region Finger

        public async Task<ApiResponse<List<FingerDto>>> GetAllFingerAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _db.Fingers
                    .AsNoTracking()
                    .OrderByDescending(x => x.id)
                    .ProjectTo<FingerDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Servos done, count={Count}", list.Count);
                return ApiResponse<List<FingerDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetAllAsync canceled");
                return ApiResponse<List<FingerDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetAllAsync failed, root={Root}", root.Message);
                return ApiResponse<List<FingerDto>>.Fail($"系统异常：{root.Message}", data: new List<FingerDto>());
            }
        }

        public async Task<ApiResponse<List<FingerDto>>> GetFingerByTaskId(string task_id,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("Query GetFingerByTaskId: task_id is empty");
                    return ApiResponse<List<FingerDto>>.BadRequest("task_id不能为空");
                }

                var list = await _db.Fingers
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<FingerDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Fingers by task_id done, task_id={task_id}, count={Count}", task_id,
                    list.Count);

                return ApiResponse<List<FingerDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetFingerByTaskIdAsync canceled, task_id={task_id}", task_id);
                return ApiResponse<List<FingerDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetFingerByTaskId failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<List<FingerDto>>.Fail($"系统异常：{root.Message}", data: new List<FingerDto>());
            }
        }

        #endregion

        #region Palm

        public async Task<ApiResponse<List<PalmDto>>> GetPalmByTaskId(string task_id,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(task_id))
                {
                    _logger.LogWarning("Query GetPalmByTaskId: task_id is empty");
                    return ApiResponse<List<PalmDto>>.BadRequest("task_id不能为空");
                }

                var list = await _db.Palms
                    .AsNoTracking()
                    .Where(x => x.task_id == task_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<PalmDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Palms by task_id done, task_id={task_id}, count={Count}", task_id,
                    list.Count);

                return ApiResponse<List<PalmDto>>.Ok(list, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("GetPalmByTaskIdAsync canceled, task_id={task_id}", task_id);
                return ApiResponse<List<PalmDto>>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "GetPalmByTaskId failed, task_id={task_id}, root={Root}", task_id,
                    root.Message);
                return ApiResponse<List<PalmDto>>.Fail($"系统异常：{root.Message}", data: new List<PalmDto>());
            }
        }

        public async Task<ApiResponse<bool>> UpdatePalm(UpdatePalmDto dto, CancellationToken ct = default)
        {
            try
            {
                if (dto == null) return ApiResponse<bool>.BadRequest("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.palm_id))
                    return ApiResponse<bool>.BadRequest("palm_id不能为空");

                var info = PartCodeHelper.Parse(dto.palm_id);
                if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<bool>.Fail("palm_id编码错误", data: false);

                // ✅ 建议先取出来，避免表达式里出现 DateTime.Now（有些 provider 不好翻译）
                var now = DateTime.Now;

                var rows = await _db.Palms
                    .Where(t => t.palm_id == dto.palm_id)
                    .ExecuteUpdateAsync(setters => setters
                            // ✅ 必更新
                            .SetProperty(x => x.updated_at, now)

                            // ✅ string：没内容就保持原值
                            .SetProperty(x => x.task_id,
                                x => string.IsNullOrWhiteSpace(dto.task_id) ? x.task_id : dto.task_id)
                            .SetProperty(x => x.operator_id,
                                x => string.IsNullOrWhiteSpace(dto.operator_id) ? x.operator_id : dto.operator_id)
                            .SetProperty(x => x.remarks,
                                x => string.IsNullOrWhiteSpace(dto.remarks) ? x.remarks : dto.remarks)

                            // ✅ nullable：没值就保持原值
                            .SetProperty(x => x.is_qualified,
                                x => dto.is_qualified.HasValue ? dto.is_qualified.Value : x.is_qualified)
                        , ct);

                if (rows == 0)
                    return ApiResponse<bool>.Fail("未找到对应 palm_id 的记录", data: false);

                _logger.LogInformation("Update Palms done, palm_id={palm_id}, rows={rows}", dto.palm_id, rows);
                return ApiResponse<bool>.Ok(true, "OK");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("UpdatePalm canceled, palm_id={palm_id}", dto?.palm_id);
                return ApiResponse<bool>.Canceled();
            }
            catch (Exception ex)
            {
                var root = ex.Root();
                _logger.LogError(ex, "UpdatePalm failed, palm_id={palm_id}, root={Root}", dto?.palm_id, root.Message);
                return ApiResponse<bool>.Fail($"系统异常：{root.Message}", data: false);
            }
        }

        #endregion
    }
}