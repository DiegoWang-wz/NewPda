using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DexRobotPDA.ApiResponses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;

namespace DexRobotPDA.Services
{
    public interface ITasksService
    {
        Task<ApiResponse<List<FullTaskDataDto>>> GetFullTaskDataAsync(string taskId, CancellationToken ct = default);
    }

    public class TasksService : ITasksService
    {
        private readonly DailyDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<TasksService> _logger;

        public TasksService(DailyDbContext db, IMapper mapper, ILogger<TasksService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<List<FullTaskDataDto>>> GetFullTaskDataAsync(string taskId, CancellationToken ct = default)
        {
            var response = new ApiResponse<List<FullTaskDataDto>>();

            try
            {
                if (string.IsNullOrWhiteSpace(taskId))
                {
                    response.ResultCode = -1;
                    response.Msg = "taskId不能为空";
                    response.ResultData = null;
                    return response;
                }

                // 1) task
                var task = await _db.ProductTasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.task_id == taskId, ct);

                if (task == null)
                {
                    response.ResultCode = -1;
                    response.Msg = "Task not found";
                    response.ResultData = null;
                    return response;
                }

                var taskDto = _mapper.Map<ProductTaskDto>(task);

                // 2) palms
                var palms = await _db.Palms
                    .AsNoTracking()
                    .Where(p => p.task_id == taskId)
                    .ToListAsync(ct);

                if (palms.Count == 0)
                {
                    response.ResultCode = 1;
                    response.Msg = "Success (no palms)";
                    response.ResultData = new List<FullTaskDataDto>();
                    return response;
                }

                var palmIds = palms
                    .Select(p => p.palm_id)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                // 3) palm detects (Detect5)
                var palmDetects = await _db.Detect5
                    .AsNoTracking()
                    .Where(d => palmIds.Contains(d.palm_id))
                    .ToListAsync(ct);

                var palmDetectDtosByPalm = palmDetects
                    .GroupBy(x => x.palm_id)
                    .ToDictionary(
                        g => g.Key,
                        g => _mapper.Map<List<PalmCalibrateDetectDto>>(g.ToList())
                    );

                // 4) fingers
                var fingers = await _db.Fingers
                    .AsNoTracking()
                    .Where(f => palmIds.Contains(f.palm_id))
                    .ToListAsync(ct);

                var fingerIds = fingers
                    .Select(f => f.finger_id)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                // 5) finger detects (Detect4/3/2)
                var detect4List = await _db.Detect4
                    .AsNoTracking()
                    .Where(d => fingerIds.Contains(d.finger_id))
                    .ToListAsync(ct);

                var detect3List = await _db.Detect3
                    .AsNoTracking()
                    .Where(d => fingerIds.Contains(d.finger_id))
                    .ToListAsync(ct);

                var detect2List = await _db.Detect2
                    .AsNoTracking()
                    .Where(d => fingerIds.Contains(d.finger_id))
                    .ToListAsync(ct);

                var detect4ByFinger = detect4List
                    .GroupBy(x => x.finger_id)
                    .ToDictionary(g => g.Key, g => _mapper.Map<List<FingerCalibrateDetectDto>>(g.ToList()));

                var detect3ByFinger = detect3List
                    .GroupBy(x => x.finger_id)
                    .ToDictionary(g => g.Key, g => _mapper.Map<List<SplitCalibrateDetectDto>>(g.ToList()));

                var detect2ByFinger = detect2List
                    .GroupBy(x => x.finger_id)
                    .ToDictionary(g => g.Key, g => _mapper.Map<List<SplitWormDetectDto>>(g.ToList()));

                // 6) motors
                var motors = await _db.Motors
                    .AsNoTracking()
                    .Where(m => fingerIds.Contains(m.finger_id))
                    .ToListAsync(ct);

                var motorDtos = _mapper.Map<List<MotorDto>>(motors);

                var motorIds = motorDtos
                    .Select(m => m.motor_id)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                // 7) motor detects (Detect1)
                var detect1List = await _db.Detect1
                    .AsNoTracking()
                    .Where(d => motorIds.Contains(d.motor_id))
                    .ToListAsync(ct);

                var detect1ByMotor = detect1List
                    .GroupBy(x => x.motor_id)
                    .ToDictionary(g => g.Key, g => _mapper.Map<List<MotorWormDetectDto>>(g.ToList()));

                var motorDtosByFinger = motorDtos
                    .GroupBy(m => m.finger_id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 8) servos（✅ 无检测，只挂载）
                // 规则：servo.superior_id 可能是 finger_id（普通舵机）或 palm_id（旋转舵机）
                var servos = await _db.Servos
                    .AsNoTracking()
                    .Where(s => palmIds.Contains(s.superior_id) || fingerIds.Contains(s.superior_id))
                    .ToListAsync(ct);

                var servoDtos = _mapper.Map<List<ServoDto>>(servos);

                // Rotary: superior_id == palm_id（FullTaskDataDto.rotateServo 是单个）
                var rotateServoByPalm = servoDtos
                    .Where(s => !string.IsNullOrWhiteSpace(s.superior_id) && palmIds.Contains(s.superior_id))
                    .GroupBy(s => s.superior_id!)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                // Normal: superior_id == finger_id（FingerDataDto.servos 是 List<ServoDataDto>）
                var normalServosByFinger = servoDtos
                    .Where(s => !string.IsNullOrWhiteSpace(s.superior_id) && fingerIds.Contains(s.superior_id))
                    .GroupBy(s => s.superior_id!)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => new ServoDataDto { servo = x }).ToList()
                    );

                // 9) 组装结果
                var fingersByPalm = fingers
                    .GroupBy(f => f.palm_id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var result = new List<FullTaskDataDto>();

                foreach (var palm in palms)
                {
                    var pid = palm.palm_id ?? "";

                    var palmDto = _mapper.Map<PalmDto>(palm);

                    var palmDetectDtos = palmDetectDtosByPalm.TryGetValue(pid, out var pd)
                        ? pd
                        : new List<PalmCalibrateDetectDto>();

                    var rotateServo = rotateServoByPalm.TryGetValue(pid, out var rs)
                        ? rs
                        : null;

                    var fingerDataList = new List<FingerDataDto>();

                    if (fingersByPalm.TryGetValue(pid, out var palmFingers))
                    {
                        foreach (var finger in palmFingers)
                        {
                            var fid = finger.finger_id ?? "";

                            var fDetect4 = detect4ByFinger.TryGetValue(fid, out var d4) ? d4 : new List<FingerCalibrateDetectDto>();
                            var fDetect3 = detect3ByFinger.TryGetValue(fid, out var d3) ? d3 : new List<SplitCalibrateDetectDto>();
                            var fDetect2 = detect2ByFinger.TryGetValue(fid, out var d2) ? d2 : new List<SplitWormDetectDto>();

                            // motors + detect1
                            var motorDataWithDetect = new List<MotorDataDto>();
                            if (motorDtosByFinger.TryGetValue(fid, out var fMotors))
                            {
                                foreach (var m in fMotors)
                                {
                                    var mid = m.motor_id ?? "";
                                    var mDetect1 = detect1ByMotor.TryGetValue(mid, out var d1) ? d1 : new List<MotorWormDetectDto>();

                                    motorDataWithDetect.Add(new MotorDataDto
                                    {
                                        motor = m,
                                        detects = mDetect1
                                    });
                                }
                            }

                            // ✅ 普通舵机（绑定 finger），无检测
                            var servoDataList = normalServosByFinger.TryGetValue(fid, out var ss)
                                ? ss
                                : new List<ServoDataDto>();

                            fingerDataList.Add(new FingerDataDto
                            {
                                finger = _mapper.Map<FingerDto>(finger),
                                motors = motorDataWithDetect,
                                servos = servoDataList,
                                detect4 = fDetect4,
                                detect3 = fDetect3,
                                detect2 = fDetect2
                            });
                        }
                    }

                    result.Add(new FullTaskDataDto
                    {
                        task = taskDto,
                        palm = palmDto,
                        rotateServo = rotateServo,      // ✅ 单个（可能为 null）
                        fingers = fingerDataList,
                        detects = palmDetectDtos
                    });
                }

                response.ResultCode = 1;
                response.Msg = "Success";
                response.ResultData = result;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "获取完整任务数据时发生错误 taskId={TaskId}", taskId);
                response.ResultCode = -1;
                response.Msg = "Error";
                response.ResultData = null;
                return response;
            }
        }
    }
}
