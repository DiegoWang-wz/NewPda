using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DexRobotPDA.Services
{
    public class DX023Service : IDX023Service
    {
        private readonly DailyDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<DX023Service> _logger;

        public DX023Service(DailyDbContext db, IMapper mapper, ILogger<DX023Service> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        // ==========================
        // Query
        // ==========================
        public async Task<List<ServoDto>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Query Servos start");
            try
            {
                var list = await _db.Servos
                    .AsNoTracking()
                    .OrderByDescending(x => x.id)
                    .ProjectTo<ServoDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Servos done, count={Count}", list.Count);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query Servos failed");
                throw;
            }
        }

        public async Task<ServoDto?> GetServoByIdAsync(string servo_id, CancellationToken ct = default)
        {
            _logger.LogInformation("Query Servo start, servo_id={ServoId}", servo_id);

            try
            {
                var entity = await _db.Servos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Servo_id == servo_id, ct);

                if (entity == null)
                {
                    _logger.LogWarning("Query Servo not found, servo_id={ServoId}", servo_id);
                    return null;
                }

                return _mapper.Map<ServoDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query Servo failed, servo_id={ServoId}", servo_id);
                throw;
            }
        }

        public async Task<List<ServoDto>> GetFingerDetailAsync(string superior_id, CancellationToken ct = default)
        {
            _logger.LogInformation("Query Servos by superior_id start, superior_id={SuperiorId}", superior_id);

            try
            {
                if (string.IsNullOrWhiteSpace(superior_id))
                {
                    _logger.LogWarning("Query Servos by superior_id: superior_id is empty");
                    return new List<ServoDto>();
                }

                var list = await _db.Servos
                    .AsNoTracking()
                    .Where(x => x.superior_id == superior_id)
                    .OrderByDescending(x => x.id)
                    .ProjectTo<ServoDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(ct);

                _logger.LogInformation("Query Servos by superior_id done, superior_id={SuperiorId}, count={Count}",
                    superior_id, list.Count);

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query Servos by superior_id failed, superior_id={SuperiorId}", superior_id);
                throw;
            }
        }

        // ==========================
        // Write (return ApiResponse<bool> so frontend can see Msg)
        // ==========================
        public async Task<ApiResponse<bool>> UnbindServoAsync(string servo_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(servo_id))
                    return FailBool("servo_id不能为空");

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
                    return FailBool($"Servo不存在：{servo_id}");
                }

                return OkBool(true, "OK");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("UnbindServoAsync canceled, servo_id={ServoId}", servo_id);
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
            catch (Exception ex)
            {
                var root = GetRootException(ex);
                _logger.LogError(ex, "UnbindServoAsync failed, servo_id={ServoId}, root={Root}", servo_id, root.Message);
                return FailBool($"系统异常：{root.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RebindServoAsync(RebindServoDto dto, CancellationToken ct = default)
        {
            try
            {
                if (dto == null) return FailBool("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    return FailBool("servo_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.superior_id))
                    return FailBool("superior_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.task_id))
                    return FailBool("task_id不能为空");

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
                    return FailBool($"Servo不存在：{dto.servo_id}");
                }

                return OkBool(true, "OK");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("RebindServoAsync canceled, servo_id={ServoId}", dto?.servo_id);
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
            catch (Exception ex)
            {
                var root = GetRootException(ex);
                _logger.LogError(ex, "RebindServoAsync failed, servo_id={ServoId}, root={Root}", dto?.servo_id, root.Message);
                return FailBool($"系统异常：{root.Message}");
            }
        }

        public async Task<ApiResponse<bool>> AddServoAsync(AddServoDto dto, CancellationToken ct = default)
        {
            try
            {
                if (dto == null) return FailBool("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    return FailBool("servo_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.operator_id))
                    return FailBool("operator_id不能为空");

                var info = PartCodeHelper.Parse(dto.servo_id);
                if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return FailBool("servo_id编码错误");

                dto.type = info.Result switch
                {
                    "Servo" => 1,
                    "RotaryServo" => 2,
                    _ => 0
                };
                if (dto.type == 0)
                    return FailBool("servo类型不支持");

                // 主键不允许重复新增
                var exists = await _db.Servos.AsNoTracking().AnyAsync(x => x.Servo_id == dto.servo_id, ct);
                if (exists)
                    return FailBool($"servo_id已存在：{dto.servo_id}");

                var entity = _mapper.Map<ServoModel>(dto);
                if (entity.created_at == default)
                    entity.created_at = DateTime.Now;

                _db.Servos.Add(entity);
                var rows = await _db.SaveChangesAsync(ct);

                _logger.LogInformation("AddServoAsync success, inserted_rows={Rows}, new_id={Id}", rows, entity.id);

                return rows > 0 ? OkBool(true, "OK") : FailBool("保存失败");
            }
            catch (DbUpdateException dbEx)
            {
                var root = GetRootException(dbEx);
                _logger.LogError(dbEx, "AddServoAsync DbUpdateException, root={Root}", root.Message);
                return FailBool($"保存失败：{root.Message}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("AddServoAsync canceled, servo_id={ServoId}", dto?.servo_id);
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
            catch (Exception ex)
            {
                var root = GetRootException(ex);
                _logger.LogError(ex, "AddServoAsync failed, root={Root}", root.Message);
                return FailBool($"系统异常：{root.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ServoBindFingerAsync(ServoBindFingerDto dto, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // ========= 参数校验（返回给前端）=========
                if (dto == null) return FailBool("请求体为空");

                if (string.IsNullOrWhiteSpace(dto.task_id))
                    return FailBool("task_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.operator_id))
                    return FailBool("operator_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.finger_id))
                    return FailBool("finger_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.servo_id1))
                    return FailBool("servo_id1不能为空");

                // ========= 校验 finger 编码 =========
                {
                    var info = PartCodeHelper.Parse(dto.finger_id);
                    if (info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                        return FailBool("finger编码错误");
                }

                // ========= servo1 =========
                var servo1Info = PartCodeHelper.Parse(dto.servo_id1);
                if (servo1Info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    return FailBool("servo_id1编码错误");

                var servo1Type = servo1Info.Result switch
                {
                    "Servo" => 1,
                    "RotaryServo" => 2,
                    _ => 0
                };
                if (servo1Type == 0) return FailBool("servo_id1类型不支持");

                var servo1 = new AddServoDto
                {
                    task_id = dto.task_id,
                    operator_id = dto.operator_id,
                    servo_id = dto.servo_id1,
                    superior_id = dto.finger_id,
                    remarks = dto.remarks,
                    type = servo1Type
                };

                // ========= servo2（可选）=========
                AddServoDto? servo2 = null;
                if (!string.IsNullOrWhiteSpace(dto.servo_id2))
                {
                    var servo2Info = PartCodeHelper.Parse(dto.servo_id2);
                    if (servo2Info.KindName.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                        return FailBool("servo_id2编码错误");

                    var servo2Type = servo2Info.Result switch
                    {
                        "Servo" => 1,
                        "RotaryServo" => 2,
                        _ => 0
                    };
                    if (servo2Type == 0) return FailBool("servo_id2类型不支持");

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

                // ========= finger =========
                var fingerDto = new AddFingerDto
                {
                    task_id = dto.task_id,
                    operator_id = dto.operator_id,
                    finger_id = dto.finger_id,
                    remarks = dto.remarks,
                    type = string.IsNullOrWhiteSpace(dto.servo_id2) ? 3 : 4 // 3=单舵机，4=双舵机
                };

                // ========= 主键重复检查（业务错误）=========
                var fingerExists = await _db.Fingers.AsNoTracking().AnyAsync(x => x.finger_id == dto.finger_id, ct);
                if (fingerExists)
                {
                    await tx.RollbackAsync(ct);
                    return FailBool($"finger_id已存在：{dto.finger_id}");
                }

                var servo1Exists = await _db.Servos.AsNoTracking().AnyAsync(x => x.Servo_id == dto.servo_id1, ct);
                if (servo1Exists)
                {
                    await tx.RollbackAsync(ct);
                    return FailBool($"servo_id1已存在：{dto.servo_id1}");
                }

                if (servo2 != null)
                {
                    var servo2Exists = await _db.Servos.AsNoTracking().AnyAsync(x => x.Servo_id == dto.servo_id2, ct);
                    if (servo2Exists)
                    {
                        await tx.RollbackAsync(ct);
                        return FailBool($"servo_id2已存在：{dto.servo_id2}");
                    }
                }

                // ========= 入库（同一事务）=========
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

                return OkBool(true, "绑定成功");
            }
            catch (DbUpdateException dbEx)
            {
                try { await tx.RollbackAsync(ct); } catch (Exception rbEx) { _logger.LogError(rbEx, "ServoBindFingerAsync rollback failed"); }

                var root = GetRootException(dbEx);
                _logger.LogError(dbEx,
                    "ServoBindFingerAsync DbUpdateException rolled back. root={RootMessage}, task_id={TaskId}, finger_id={FingerId}, servo1={Servo1}, servo2={Servo2}",
                    root.Message, dto?.task_id, dto?.finger_id, dto?.servo_id1, dto?.servo_id2);

                return FailBool($"保存失败：{root.Message}");
            }
            catch (OperationCanceledException)
            {
                try { await tx.RollbackAsync(ct); } catch { /* ignore */ }
                _logger.LogWarning("ServoBindFingerAsync canceled, task_id={TaskId}, finger_id={FingerId}", dto?.task_id, dto?.finger_id);
                return new ApiResponse<bool> { ResultCode = -2, Msg = "Request canceled", ResultData = false };
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(ct); } catch (Exception rbEx) { _logger.LogError(rbEx, "ServoBindFingerAsync rollback failed"); }

                var root = GetRootException(ex);
                _logger.LogError(ex,
                    "ServoBindFingerAsync failed and rolled back. root={RootMessage}, task_id={TaskId}, finger_id={FingerId}, servo1={Servo1}, servo2={Servo2}",
                    root.Message, dto?.task_id, dto?.finger_id, dto?.servo_id1, dto?.servo_id2);

                return FailBool($"系统异常：{root.Message}");
            }
        }

        // ==========================
        // Helpers
        // ==========================
        private static ApiResponse<bool> OkBool(bool data, string msg)
            => new ApiResponse<bool> { ResultCode = 1, Msg = msg, ResultData = data };

        private static ApiResponse<bool> FailBool(string msg)
            => new ApiResponse<bool> { ResultCode = 0, Msg = msg, ResultData = false };

        private static Exception GetRootException(Exception ex)
        {
            var root = ex;
            while (root.InnerException != null) root = root.InnerException;
            return root;
        }
    }
}
