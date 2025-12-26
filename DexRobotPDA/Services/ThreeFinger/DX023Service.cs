using AutoMapper;
using AutoMapper.QueryableExtensions;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;
using Microsoft.EntityFrameworkCore;

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

                // 如果你有 AutoMapper：
                return _mapper.Map<ServoDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query Servo failed, servo_id={ServoId}", servo_id);
                throw;
            }
        }
        
        public async Task<bool> AddServoAsync(AddServoDto dto)
        {
            _logger.LogInformation("AddServoAsync start, servo_id={ServoId}, task_id={TaskId}",
                dto.servo_id, dto.task_id);

            try
            {
                // 1) 最基本校验（避免插入必填字段为空）
                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    throw new ArgumentException("servo_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.operator_id))
                    throw new ArgumentException("operator_id不能为空");

                // 2) DTO -> Entity
                var entity = _mapper.Map<ServoModel>(dto);

                // 3) 兜底时间字段（根据你的表设计）
                if (entity.created_at == default)
                    entity.created_at = DateTime.Now;

                // 4) 新增 & 保存
                _db.Servos.Add(entity);
                var rows = await _db.SaveChangesAsync();

                _logger.LogInformation("BindServoAsync success, inserted_rows={Rows}, new_id={Id}",
                    rows, entity.id);

                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BindServoAsync failed");
                throw;
            }
        }
        
        public async Task<bool> UnbindServoAsync(string servo_id, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(servo_id))
                    throw new ArgumentException("servo_id不能为空", nameof(servo_id));

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
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnbindServoAsync failed, servo_id={ServoId}", servo_id);
                throw;
            }
        }
        
        public async Task<bool> RebindServoAsync(RebindServoDto dto, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    throw new ArgumentException("servo_id不能为空", nameof(dto.servo_id));
                if (string.IsNullOrWhiteSpace(dto.superior_id))
                    throw new ArgumentException("superior_id不能为空", nameof(dto.superior_id));
                if (string.IsNullOrWhiteSpace(dto.task_id))
                    throw new ArgumentException("task_id不能为空", nameof(dto.task_id));

                var rows = await _db.Servos
                    .Where(t => t.Servo_id == dto.servo_id)
                    .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.superior_id, dto.superior_id)
                            .SetProperty(x => x.task_id, dto.task_id)
                            .SetProperty(x => x.updated_at, DateTime.Now),
                        ct);

                if (rows == 0)
                {
                    _logger.LogWarning("UnbindServoAsync: Servo not found, servo_id={ServoId}", dto.servo_id);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnbindServoAsync failed, servo_id={ServoId}", dto.servo_id);
                throw;
            }
        }


        public async Task<bool> ServoBindFingerAsync(ServoBindFingerDto dto)
        {
            _logger.LogInformation("ServoBindFingerAsync start, servo_id={ServoId}, task_id={TaskId}",
                dto.servo_id, dto.task_id);

            try
            {
                // 1) 最基本校验（避免插入必填字段为空）
                if (string.IsNullOrWhiteSpace(dto.servo_id))
                    throw new ArgumentException("servo_id不能为空");
                if (string.IsNullOrWhiteSpace(dto.operator_id))
                    throw new ArgumentException("operator_id不能为空");

                // 2) DTO -> Entity
                var entity = _mapper.Map<ServoModel>(dto);

                // 3) 兜底时间字段（根据你的表设计）
                if (entity.created_at == default)
                    entity.created_at = DateTime.Now;

                // 4) 新增 & 保存
                _db.Servos.Add(entity);
                var rows = await _db.SaveChangesAsync();

                _logger.LogInformation("ServoBindFingerAsync success, inserted_rows={Rows}, new_id={Id}",
                    rows, entity.id);

                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ServoBindFingerAsync failed");
                throw;
            }
        }

    }
}