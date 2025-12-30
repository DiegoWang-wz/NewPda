using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.DataModel;

namespace DexRobotPDA.Services
{
    public sealed record SimplePartInfo(
        string Raw,
        string KindName,
        string Result,
        IReadOnlyDictionary<string, string> Fields
    );

    /// <summary>
    /// 每一层节点（Bottom -> Top）
    /// </summary>
    public sealed record TraceNodeDto(
        int Level,
        string Kind,              // Motor / Servo / Finger / Palm / Task
        string? Id,               // 해당层的 id
        string? Display,          // UI展示用
        string? ParentKind,       // 上一层 kind
        string? ParentId,         // 上一层 id
        string? Relation          // 关联字段名：superior_id / finger_id / palm_id / task_id
    );

    /// <summary>
    /// Trace result: 平铺字段 + Nodes 层级链路
    /// </summary>
    public sealed record TracePathDto(
        string ScannedCode,
        string IdentifiedResult,

        // 平铺（兼容旧代码）
        string? MotorId,
        string? ServoId,
        string? FingerId,
        string? PalmId,
        string? PalmSide,
        string? TaskId,

        // 层级链路（凸显上下关系）
        IReadOnlyList<TraceNodeDto> Nodes
    );

    public static class PartCodeHelper
    {
        // =========================
        // 识别规则（你指定）
        // =========================
        private const string MotorPrefix = "M11021G27W";
        private const string MoPrefix = "MO-";
        private const string ServoPrefix = "R003D0";
        private const string RotaryServoPrefix = "S009F0";

        private static readonly HashSet<string> ProductLines = new(StringComparer.OrdinalIgnoreCase)
        {
            "DX021",
            "DX023"
        };

        // =========================
        // ★ 实体字段候选名
        // =========================
        private static readonly string[] MotorIdProps = { "motor_id", "Motor_id", "MotorId" };
        private static readonly string[] ServoIdProps = { "servo_id", "Servo_id", "ServoId" };
        private static readonly string[] FingerIdProps = { "finger_id", "Finger_id", "FingerId" };
        private static readonly string[] PalmIdProps = { "palm_id", "Palm_id", "PalmId" };

        // ✅ 你确认：Servo 表用 superior_id 指向上级（Finger 或 Palm）
        private static readonly string[] ServoSuperiorIdProps = { "superior_id", "Superior_id", "SuperiorId" };

        // ✅ servo.type（1=普通舵机，2=旋转舵机）——用于兜底判断
        private static readonly string[] ServoTypeProps = { "type", "Type" };

        // Motor -> finger（motor 可能是 finger_id 或 superior_id）
        private static readonly string[] MotorFingerIdProps = { "finger_id", "Finger_id", "FingerId", "superior_id", "Superior_id", "SuperiorId" };

        private static readonly string[] FingerPalmIdProps = { "palm_id", "Palm_id", "PalmId" };

        private static readonly string[] PalmTaskIdProps =
        {
            "task_id", "Task_id", "TaskId",
            "producttask_id", "ProductTaskId", "ProductTask_id"
        };

        private static readonly string[] TaskIdProps =
        {
            "task_id", "Task_id", "TaskId",
            "producttask_id", "ProductTaskId", "ProductTask_id",
            "Id", "ID"
        };

        // =========================
        // 1) 只负责识别红色码
        // =========================
        public static SimplePartInfo Parse(string? code)
        {
            var raw = (code ?? "").Trim();
            if (string.IsNullOrEmpty(raw))
                return Unknown(raw, "Empty");

            var s = raw.ToUpperInvariant();

            if (s.StartsWith(MotorPrefix, StringComparison.Ordinal))
                return New(raw, "Motor", "Motor",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["prefix"] = MotorPrefix });

            if (s.StartsWith(ServoPrefix, StringComparison.Ordinal))
                return New(raw, "Servo", "Servo",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["prefix"] = ServoPrefix,
                        ["servo_type"] = "Normal"
                    });

            if (s.StartsWith(RotaryServoPrefix, StringComparison.Ordinal))
                return New(raw, "Servo", "RotaryServo",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["prefix"] = RotaryServoPrefix,
                        ["servo_type"] = "Rotary"
                    });

            if (s.StartsWith(MoPrefix, StringComparison.Ordinal))
            {
                var parts = s.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                    return Unknown(raw, $"MO invalid segments={parts.Length}");

                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["p0"] = parts.ElementAtOrDefault(0) ?? "",
                    ["p1"] = parts.ElementAtOrDefault(1) ?? "",
                    ["p2"] = parts.ElementAtOrDefault(2) ?? "",
                    ["p3"] = parts.ElementAtOrDefault(3) ?? ""
                };
                if (parts.Length >= 5) fields["p4"] = parts[4];

                if (parts.Length == 5)
                {
                    var t = parts[3];
                    fields["type_code"] = t;

                    var result = t switch
                    {
                        "FL" => "Finger-F-L",
                        "FR" => "Finger-F-R",
                        "ML" => "Finger-M-L",
                        "MR" => "Finger-M-R",
                        "F0" => "Finger-F",
                        "M0" => "Finger-M",
                        "D0" => "Finger-D",
                        _ => $"Finger({t})"
                    };

                    var side = result.EndsWith("-L", StringComparison.Ordinal) ? "L"
                             : result.EndsWith("-R", StringComparison.Ordinal) ? "R"
                             : "";
                    if (!string.IsNullOrEmpty(side)) fields["side"] = side;

                    fields["finger_kind"] =
                        result.Contains("Finger-F", StringComparison.Ordinal) ? "F" :
                        result.Contains("Finger-M", StringComparison.Ordinal) ? "M" :
                        result.Contains("Finger-D", StringComparison.Ordinal) ? "D" : "";

                    return New(raw, "Finger", result, fields);
                }

                if (parts.Length == 4)
                    return New(raw, "Finger", "Finger", fields);

                return New(raw, "Finger", $"Finger(segments={parts.Length})", fields);
            }

            if (s.StartsWith("DX", StringComparison.Ordinal))
            {
                var parts = s.Split('-', StringSplitOptions.RemoveEmptyEntries);

                // Palm: DX021-60001/60002 etc.
                if (parts.Length == 2)
                {
                    var line = parts[0];
                    var sku = parts[1];
                    if (ProductLines.Contains(line) && (sku == "60001" || sku == "60002"))
                    {
                        var side = sku == "60001" ? "L" : "R";
                        return New(raw, "Palm", side == "L" ? "Palm-L" : "Palm-R",
                            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["product_line"] = line,
                                ["sku"] = sku,
                                ["side"] = side
                            });
                    }
                }

                // Product line (DX021/DX023...)
                foreach (var pl in ProductLines)
                {
                    if (s.StartsWith(pl, StringComparison.Ordinal))
                    {
                        return New(raw, "Product", pl,
                            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["product_line"] = pl });
                    }
                }
            }

            return Unknown(raw, "No match");
        }

        // =========================
        // 2) 溯源：返回“层级链路”，更凸显上下关系
        // =========================
        public static async Task<TracePathDto> TraceAsync(DailyDbContext db, string scannedCode, CancellationToken ct = default)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));

            var info = Parse(scannedCode);
            var rawTrim = (scannedCode ?? "").Trim();
            var upperTrim = rawTrim.ToUpperInvariant();

            // Nodes：Bottom -> Top
            var nodes = new List<TraceNodeDto>();
            int level = 0;

            // 平铺字段（兼容）
            string? motorId = null;
            string? servoId = null;
            string? fingerId = null;
            string? palmId = null;
            string? palmSide = null;
            string? taskId = null;

            // 产品不溯源
            if (string.Equals(info.KindName, "Product", StringComparison.OrdinalIgnoreCase))
            {
                return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);
            }

            // ============= Palm =============
            if (string.Equals(info.KindName, "Palm", StringComparison.OrdinalIgnoreCase))
            {
                var palm = await FindByAnyAsync(db.Palms.AsNoTracking(), PalmIdProps, rawTrim, upperTrim, ct);
                if (palm is null)
                    return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);

                palmId = GetStringByAny(palm, PalmIdProps);
                palmSide = InferPalmSideFromPalmId(palmId);

                nodes.Add(new TraceNodeDto(level++, "Palm", palmId, $"Palm {palmId} ({palmSide ?? "?"})", null, null, null));

                var (tid, taskNode) = await TryGetTaskFromPalmAsync(db, palm, palmId, ct);
                taskId = tid;
                if (taskNode != null) nodes.Add(taskNode);

                return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);
            }

            // ============= Finger =============
            if (string.Equals(info.KindName, "Finger", StringComparison.OrdinalIgnoreCase))
            {
                var finger = await FindByAnyAsync(db.Fingers.AsNoTracking(), FingerIdProps, rawTrim, upperTrim, ct);
                if (finger is null)
                    return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);

                fingerId = GetStringByAny(finger, FingerIdProps);
                nodes.Add(new TraceNodeDto(level++, "Finger", fingerId, $"Finger {fingerId}", null, null, null));

                var (pid, pside, palmNode, taskNode) = await TryGetPalmAndTaskFromFingerAsync(db, finger, fingerId, ct);
                palmId = pid;
                palmSide = pside;
                if (palmNode != null) nodes.Add(palmNode);
                if (taskNode != null) { taskId = taskNode.Id; nodes.Add(taskNode); }

                return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);
            }

            // ============= Motor =============
            if (string.Equals(info.KindName, "Motor", StringComparison.OrdinalIgnoreCase))
            {
                var motor = await FindByAnyAsync(db.Motors.AsNoTracking(), MotorIdProps, rawTrim, upperTrim, ct);
                if (motor is null)
                    return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);

                motorId = GetStringByAny(motor, MotorIdProps);
                nodes.Add(new TraceNodeDto(level++, "Motor", motorId, $"Motor {motorId}", null, null, null));

                // motor -> finger_id/superior_id
                var fingerKey = GetStringByAny(motor, MotorFingerIdProps);
                if (!string.IsNullOrWhiteSpace(fingerKey))
                {
                    var finger = await FindByAnyAsync(db.Fingers.AsNoTracking(), FingerIdProps, fingerKey.Trim(), fingerKey.Trim().ToUpperInvariant(), ct);
                    if (finger != null)
                    {
                        fingerId = GetStringByAny(finger, FingerIdProps);
                        nodes.Add(new TraceNodeDto(level++, "Finger", fingerId, $"Finger {fingerId}", "Motor", motorId, "finger_id/superior_id"));

                        var (pid, pside, palmNode, taskNode) = await TryGetPalmAndTaskFromFingerAsync(db, finger, fingerId, ct);
                        palmId = pid;
                        palmSide = pside;
                        if (palmNode != null) nodes.Add(palmNode);
                        if (taskNode != null) { taskId = taskNode.Id; nodes.Add(taskNode); }
                    }
                }

                return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);
            }

            // ============= Servo（普通/旋转） =============
            if (string.Equals(info.KindName, "Servo", StringComparison.OrdinalIgnoreCase))
            {
                var servo = await FindByAnyAsync(db.Servos.AsNoTracking(), ServoIdProps, rawTrim, upperTrim, ct);
                if (servo is null)
                    return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);

                servoId = GetStringByAny(servo, ServoIdProps);

                // 判定是否旋转舵机：优先 Parse.Result，否则兜底读 servo.type==2
                var isRotary = string.Equals(info.Result, "RotaryServo", StringComparison.OrdinalIgnoreCase);
                if (!isRotary)
                {
                    var servoTypeStr = GetStringByAny(servo, ServoTypeProps);
                    if (int.TryParse(servoTypeStr, out var st) && st == 2) isRotary = true;
                }

                nodes.Add(new TraceNodeDto(level++, "Servo", servoId, isRotary ? $"Servo(Rotary) {servoId}" : $"Servo {servoId}", null, null, null));

                // servo -> superior_id
                var supKey = GetStringByAny(servo, ServoSuperiorIdProps);
                if (string.IsNullOrWhiteSpace(supKey))
                    return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);

                supKey = supKey.Trim();
                var supUpper = supKey.ToUpperInvariant();

                if (isRotary)
                {
                    // 旋转舵机：superior_id -> Palm
                    var palm = await FindByAnyAsync(db.Palms.AsNoTracking(), PalmIdProps, supKey, supUpper, ct);
                    if (palm != null)
                    {
                        palmId = GetStringByAny(palm, PalmIdProps);
                        palmSide = InferPalmSideFromPalmId(palmId);

                        nodes.Add(new TraceNodeDto(level++, "Palm", palmId, $"Palm {palmId} ({palmSide ?? "?"})", "Servo", servoId, "superior_id"));

                        var (tid, taskNode) = await TryGetTaskFromPalmAsync(db, palm, palmId, ct);
                        taskId = tid;
                        if (taskNode != null) nodes.Add(taskNode);
                    }
                }
                else
                {
                    // 普通舵机：superior_id -> Finger
                    var finger = await FindByAnyAsync(db.Fingers.AsNoTracking(), FingerIdProps, supKey, supUpper, ct);
                    if (finger != null)
                    {
                        fingerId = GetStringByAny(finger, FingerIdProps);
                        nodes.Add(new TraceNodeDto(level++, "Finger", fingerId, $"Finger {fingerId}", "Servo", servoId, "superior_id"));

                        var (pid, pside, palmNode, taskNode) = await TryGetPalmAndTaskFromFingerAsync(db, finger, fingerId, ct);
                        palmId = pid;
                        palmSide = pside;
                        if (palmNode != null) nodes.Add(palmNode);
                        if (taskNode != null) { taskId = taskNode.Id; nodes.Add(taskNode); }
                    }
                }

                return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);
            }

            return new TracePathDto(scannedCode, info.Result, motorId, servoId, fingerId, palmId, palmSide, taskId, nodes);
        }

        // =========================
        // Finger -> Palm -> Task (节点化)
        // =========================
        private static async Task<(string? PalmId, string? PalmSide, TraceNodeDto? PalmNode, TraceNodeDto? TaskNode)>
            TryGetPalmAndTaskFromFingerAsync(DailyDbContext db, object finger, string? fingerId, CancellationToken ct)
        {
            var palmKey = GetStringByAny(finger, FingerPalmIdProps);
            if (string.IsNullOrWhiteSpace(palmKey))
                return (null, null, null, null);

            palmKey = palmKey.Trim();
            var palm = await FindByAnyAsync(db.Palms.AsNoTracking(), PalmIdProps, palmKey, palmKey.ToUpperInvariant(), ct);
            if (palm is null)
                return (null, null, null, null);

            var palmId = GetStringByAny(palm, PalmIdProps);
            var palmSide = InferPalmSideFromPalmId(palmId);

            var palmNode = new TraceNodeDto(
                Level: -1, // Level 由上层 TraceAsync 统一赋值，这里不关心
                Kind: "Palm",
                Id: palmId,
                Display: $"Palm {palmId} ({palmSide ?? "?"})",
                ParentKind: "Finger",
                ParentId: fingerId,
                Relation: "palm_id"
            );

            var (taskId, taskNode) = await TryGetTaskFromPalmAsync(db, palm, palmId, ct);
            return (palmId, palmSide, palmNode, taskNode);
        }

        private static async Task<(string? TaskId, TraceNodeDto? TaskNode)>
            TryGetTaskFromPalmAsync(DailyDbContext db, object palm, string? palmId, CancellationToken ct)
        {
            var taskKey = GetStringByAny(palm, PalmTaskIdProps);
            if (string.IsNullOrWhiteSpace(taskKey))
                return (null, null);

            taskKey = taskKey.Trim();
            var task = await FindByKeyAsync(db.ProductTasks.AsNoTracking(), TaskIdProps, taskKey, ct);
            if (task is null)
                return (null, null);

            var taskId = GetStringByAny(task, TaskIdProps) ?? taskKey;

            var taskNode = new TraceNodeDto(
                Level: -1,
                Kind: "Task",
                Id: taskId,
                Display: $"Task {taskId}",
                ParentKind: "Palm",
                ParentId: palmId,
                Relation: "task_id"
            );

            return (taskId, taskNode);
        }

        // =========================================================
        // Generic find helpers
        // =========================================================
        private static async Task<object?> FindByAnyAsync<T>(
            IQueryable<T> query,
            string[] propCandidates,
            string rawValue,
            string upperValue,
            CancellationToken ct) where T : class
        {
            var v1 = rawValue;
            var v2 = upperValue;

            foreach (var prop in ResolveExistingProps(typeof(T), propCandidates))
            {
                var hit = await FirstOrDefaultByStringPropAsync(query, prop, v1, ct);
                if (hit is not null) return hit;

                if (!string.Equals(v1, v2, StringComparison.Ordinal))
                {
                    hit = await FirstOrDefaultByStringPropAsync(query, prop, v2, ct);
                    if (hit is not null) return hit;
                }
            }

            return null;
        }

        private static async Task<object?> FindByKeyAsync<T>(
            IQueryable<T> query,
            string[] keyPropCandidates,
            string keyValue,
            CancellationToken ct) where T : class
        {
            foreach (var prop in ResolveExistingProps(typeof(T), keyPropCandidates))
            {
                var pType = prop.PropertyType;
                if (pType == typeof(string))
                {
                    var hit = await FirstOrDefaultByStringPropAsync(query, prop, keyValue, ct);
                    if (hit is not null) return hit;

                    var upper = keyValue.ToUpperInvariant();
                    if (!string.Equals(keyValue, upper, StringComparison.Ordinal))
                    {
                        hit = await FirstOrDefaultByStringPropAsync(query, prop, upper, ct);
                        if (hit is not null) return hit;
                    }
                }
                else if (pType == typeof(int) || pType == typeof(int?))
                {
                    if (int.TryParse(keyValue, out var iv))
                    {
                        var hit = await FirstOrDefaultByIntPropAsync(query, prop, iv, ct);
                        if (hit is not null) return hit;
                    }
                }
                else if (pType == typeof(long) || pType == typeof(long?))
                {
                    if (long.TryParse(keyValue, out var lv))
                    {
                        var hit = await FirstOrDefaultByLongPropAsync(query, prop, lv, ct);
                        if (hit is not null) return hit;
                    }
                }
                else if (pType == typeof(Guid) || pType == typeof(Guid?))
                {
                    if (Guid.TryParse(keyValue, out var gv))
                    {
                        var hit = await FirstOrDefaultByGuidPropAsync(query, prop, gv, ct);
                        if (hit is not null) return hit;
                    }
                }
            }

            return null;
        }

        private static Task<T?> FirstOrDefaultByStringPropAsync<T>(IQueryable<T> query, System.Reflection.PropertyInfo prop, string value, CancellationToken ct)
            where T : class
        {
            var param = Expression.Parameter(typeof(T), "e");
            var member = Expression.Property(param, prop);
            var constant = Expression.Constant(value, typeof(string));
            var equal = Expression.Equal(member, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equal, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        private static Task<T?> FirstOrDefaultByIntPropAsync<T>(IQueryable<T> query, System.Reflection.PropertyInfo prop, int value, CancellationToken ct)
            where T : class
        {
            var param = Expression.Parameter(typeof(T), "e");
            var member = Expression.Property(param, prop);

            Expression constant = Expression.Constant(value, typeof(int));
            Expression body = prop.PropertyType == typeof(int?)
                ? Expression.Equal(member, Expression.Convert(constant, typeof(int?)))
                : Expression.Equal(member, constant);

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        private static Task<T?> FirstOrDefaultByLongPropAsync<T>(IQueryable<T> query, System.Reflection.PropertyInfo prop, long value, CancellationToken ct)
            where T : class
        {
            var param = Expression.Parameter(typeof(T), "e");
            var member = Expression.Property(param, prop);

            Expression constant = Expression.Constant(value, typeof(long));
            Expression body = prop.PropertyType == typeof(long?)
                ? Expression.Equal(member, Expression.Convert(constant, typeof(long?)))
                : Expression.Equal(member, constant);

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        private static Task<T?> FirstOrDefaultByGuidPropAsync<T>(IQueryable<T> query, System.Reflection.PropertyInfo prop, Guid value, CancellationToken ct)
            where T : class
        {
            var param = Expression.Parameter(typeof(T), "e");
            var member = Expression.Property(param, prop);

            Expression constant = Expression.Constant(value, typeof(Guid));
            Expression body = prop.PropertyType == typeof(Guid?)
                ? Expression.Equal(member, Expression.Convert(constant, typeof(Guid?)))
                : Expression.Equal(member, constant);

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        // =========================================================
        // Reflection helpers
        // =========================================================
        private static string? GetStringByAny(object obj, string[] propCandidates)
        {
            var t = obj.GetType();
            foreach (var p in ResolveExistingProps(t, propCandidates))
            {
                var v = p.GetValue(obj);
                if (v is null) continue;
                var s = v.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return null;
        }

        private static IEnumerable<System.Reflection.PropertyInfo> ResolveExistingProps(Type type, string[] candidates)
        {
            foreach (var name in candidates)
            {
                var p = type.GetProperty(name);
                if (p != null) yield return p;
            }

            foreach (var name in candidates)
            {
                var p = type.GetProperties()
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (p != null) yield return p;
            }
        }

        private static string? InferPalmSideFromPalmId(string? palmId)
        {
            if (string.IsNullOrWhiteSpace(palmId)) return null;
            var s = palmId.Trim().ToUpperInvariant();
            if (s.EndsWith("60001", StringComparison.Ordinal)) return "L";
            if (s.EndsWith("60002", StringComparison.Ordinal)) return "R";
            return null;
        }

        private static SimplePartInfo New(string raw, string kindName, string result, Dictionary<string, string> fields)
            => new(raw, kindName, result, fields);

        private static SimplePartInfo Unknown(string raw, string reason)
            => new(raw, "Unknown", "Unknown",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["reason"] = reason });
    }
}
