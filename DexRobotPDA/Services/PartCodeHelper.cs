using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.DataModel; // 你的 DailyDbContext 命名空间（如果不同就改这里）

namespace DexRobotPDA.Services
{
    public enum SimplePartKind
    {
        Unknown = 0,
        Product,     // DX021 / DX023
        Palm,        // Palm-L / Palm-R
        Motor,       // Motor
        Finger,      // Finger / Finger-F-L ...
        Servo,       // Servo
        RotaryServo  // RotaryServo
    }

    public sealed record SimplePartInfo(
        string Raw,
        SimplePartKind Kind,
        string KindName,
        string Result,
        IReadOnlyDictionary<string, string> Fields
    );

    /// <summary>
    /// Trace result (Bottom -> Top)
    /// </summary>
    public sealed record TraceChainDto(
        string ScannedCode,
        string IdentifiedKind,
        string IdentifiedResult,

        string? MotorId,
        string? ServoId,
        string? FingerId,
        string? PalmId,
        string? PalmSide,   // L / R if can infer
        string? TaskId,
        string? TaskNo      // 任务单号（如果表里没有 TaskNo，会尝试 TaskCode/TaskNumber）
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
        // ★ 关键：实体字段候选名（自动适配 motor_id / MotorId / Motor_id 等）
        // 你如果确定你项目里的属性名，就把候选留一个也行。
        // =========================
        private static readonly string[] MotorIdProps = { "motor_id", "Motor_id", "MotorId" };
        private static readonly string[] ServoIdProps = { "servo_id", "Servo_id", "ServoId" };
        private static readonly string[] FingerIdProps = { "finger_id", "Finger_id", "FingerId" };
        private static readonly string[] PalmIdProps  = { "palm_id",  "Palm_id",  "PalmId" };

        // 关联字段（child -> finger / finger -> palm / palm -> task）
        private static readonly string[] MotorFingerIdProps = { "finger_id", "Finger_id", "FingerId" };
        private static readonly string[] ServoFingerIdProps = { "finger_id", "Finger_id", "FingerId" };
        private static readonly string[] FingerPalmIdProps  = { "palm_id",   "Palm_id",   "PalmId" };

        private static readonly string[] PalmTaskIdProps =
        {
            "task_id", "Task_id", "TaskId",
            "producttask_id", "ProductTaskId", "ProductTask_id"
        };

        // ProductTask 的“任务单号”字段候选名
        private static readonly string[] TaskNoProps = { "task_no", "TaskNo", "TaskNO", "TaskCode", "TaskNumber" };

        // ProductTask 的“主键/ID字段”候选名（用来根据 task_id 去找 task）
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

            // Motor
            if (s.StartsWith(MotorPrefix, StringComparison.Ordinal))
                return New(raw, SimplePartKind.Motor, "Motor", "Motor",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["prefix"] = MotorPrefix });

            // Servo
            if (s.StartsWith(ServoPrefix, StringComparison.Ordinal))
                return New(raw, SimplePartKind.Servo, "Servo", "Servo",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["prefix"] = ServoPrefix });

            // RotaryServo
            if (s.StartsWith(RotaryServoPrefix, StringComparison.Ordinal))
                return New(raw, SimplePartKind.RotaryServo, "RotaryServo", "RotaryServo",
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["prefix"] = RotaryServoPrefix });

            // MO -> Finger
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
                    var t = parts[3]; // D0/F0/M0 or FL/FR/ML/MR
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

                    return New(raw, SimplePartKind.Finger, "Finger", result, fields);
                }

                if (parts.Length == 4)
                    return New(raw, SimplePartKind.Finger, "Finger", "Finger", fields);

                return New(raw, SimplePartKind.Finger, "Finger", $"Finger(segments={parts.Length})", fields);
            }

            // DX product line / palm
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
                        return New(raw, SimplePartKind.Palm, "Palm", side == "L" ? "Palm-L" : "Palm-R",
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
                        return New(raw, SimplePartKind.Product, "Product", pl,
                            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["product_line"] = pl });
                    }
                }
            }

            return Unknown(raw, "No match");
        }

        // =========================
        // 2) 溯源：从下到上查链路
        //    Motor -> Finger -> Palm -> ProductTask
        //    Servo/RotaryServo -> Finger -> Palm -> ProductTask
        //    Finger -> Palm -> ProductTask
        //    Palm -> ProductTask
        // =========================
        public static async Task<TraceChainDto> TraceAsync(DailyDbContext db, string scannedCode, CancellationToken ct = default)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));

            var info = Parse(scannedCode);
            var rawTrim = (scannedCode ?? "").Trim();
            var upperTrim = rawTrim.ToUpperInvariant();

            var dto = new TraceChainDto(
                ScannedCode: scannedCode,
                IdentifiedKind: info.KindName,
                IdentifiedResult: info.Result,
                MotorId: null,
                ServoId: null,
                FingerId: null,
                PalmId: null,
                PalmSide: null,
                TaskId: null,
                TaskNo: null
            );

            if (info.Kind == SimplePartKind.Product)
                return dto;

            if (info.Kind == SimplePartKind.Palm)
            {
                var palm = await FindByAnyAsync(db.Palms.AsNoTracking(), PalmIdProps, rawTrim, upperTrim, ct);
                if (palm is null) return dto;

                var palmId = GetStringByAny(palm, PalmIdProps);
                dto = dto with { PalmId = palmId, PalmSide = InferPalmSideFromPalmId(palmId) };

                dto = await FillTaskFromPalmAsync(db, palm, dto, ct);
                return dto;
            }

            if (info.Kind == SimplePartKind.Finger)
            {
                var finger = await FindByAnyAsync(db.Fingers.AsNoTracking(), FingerIdProps, rawTrim, upperTrim, ct);
                if (finger is null) return dto;

                var fingerId = GetStringByAny(finger, FingerIdProps);
                dto = dto with { FingerId = fingerId };

                dto = await FillPalmFromFingerAsync(db, finger, dto, ct);
                return dto;
            }

            if (info.Kind == SimplePartKind.Motor)
            {
                var motor = await FindByAnyAsync(db.Motors.AsNoTracking(), MotorIdProps, rawTrim, upperTrim, ct);
                if (motor is null) return dto;

                var motorId = GetStringByAny(motor, MotorIdProps);
                dto = dto with { MotorId = motorId };

                // motor -> finger_id
                var fingerKey = GetStringByAny(motor, MotorFingerIdProps);
                if (string.IsNullOrWhiteSpace(fingerKey)) return dto;

                var finger = await FindByAnyAsync(db.Fingers.AsNoTracking(), FingerIdProps, fingerKey.Trim(), fingerKey.Trim().ToUpperInvariant(), ct);
                if (finger is null) return dto;

                dto = dto with { FingerId = GetStringByAny(finger, FingerIdProps) };

                dto = await FillPalmFromFingerAsync(db, finger, dto, ct);
                return dto;
            }

            if (info.Kind == SimplePartKind.Servo || info.Kind == SimplePartKind.RotaryServo)
            {
                var servo = await FindByAnyAsync(db.Servos.AsNoTracking(), ServoIdProps, rawTrim, upperTrim, ct);
                if (servo is null) return dto;

                var servoId = GetStringByAny(servo, ServoIdProps);
                dto = dto with { ServoId = servoId };

                // servo -> finger_id
                var fingerKey = GetStringByAny(servo, ServoFingerIdProps);
                if (string.IsNullOrWhiteSpace(fingerKey)) return dto;

                var finger = await FindByAnyAsync(db.Fingers.AsNoTracking(), FingerIdProps, fingerKey.Trim(), fingerKey.Trim().ToUpperInvariant(), ct);
                if (finger is null) return dto;

                dto = dto with { FingerId = GetStringByAny(finger, FingerIdProps) };

                dto = await FillPalmFromFingerAsync(db, finger, dto, ct);
                return dto;
            }

            return dto;
        }

        // =========================
        // Finger -> Palm -> Task
        // =========================
        private static async Task<TraceChainDto> FillPalmFromFingerAsync(DailyDbContext db, object finger, TraceChainDto dto, CancellationToken ct)
        {
            var palmKey = GetStringByAny(finger, FingerPalmIdProps);
            if (string.IsNullOrWhiteSpace(palmKey)) return dto;

            var palm = await FindByAnyAsync(db.Palms.AsNoTracking(), PalmIdProps, palmKey.Trim(), palmKey.Trim().ToUpperInvariant(), ct);
            if (palm is null) return dto;

            var palmId = GetStringByAny(palm, PalmIdProps);
            dto = dto with { PalmId = palmId, PalmSide = InferPalmSideFromPalmId(palmId) };

            dto = await FillTaskFromPalmAsync(db, palm, dto, ct);
            return dto;
        }

        // =========================
        // Palm -> Task
        // =========================
        private static async Task<TraceChainDto> FillTaskFromPalmAsync(DailyDbContext db, object palm, TraceChainDto dto, CancellationToken ct)
        {
            var taskKey = GetStringByAny(palm, PalmTaskIdProps);
            if (string.IsNullOrWhiteSpace(taskKey)) return dto;

            // 根据 task_id 去 ProductTasks 找记录（支持 string/int 主键）
            var task = await FindByKeyAsync(db.ProductTasks.AsNoTracking(), TaskIdProps, taskKey.Trim(), ct);
            if (task is null) return dto;

            var taskId = GetStringByAny(task, TaskIdProps) ?? taskKey.Trim();
            var taskNo = GetStringByAny(task, TaskNoProps);

            dto = dto with { TaskId = taskId, TaskNo = taskNo };
            return dto;
        }

        // =========================================================
        // Generic: Find entity by string property (try several prop names)
        // =========================================================
        private static async Task<object?> FindByAnyAsync<T>(
            IQueryable<T> query,
            string[] propCandidates,
            string rawValue,
            string upperValue,
            CancellationToken ct) where T : class
        {
            // 优先尝试 rawValue（保留空格/大小写），再尝试 upperValue
            var v1 = rawValue;
            var v2 = upperValue;

            foreach (var prop in ResolveExistingProps(typeof(T), propCandidates))
            {
                // try raw
                var hit = await FirstOrDefaultByStringPropAsync(query, prop, v1, ct);
                if (hit is not null) return hit;

                // try upper (fallback)
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

        // =========================================================
        // Build translated EF predicate: e => e.Prop == value
        // =========================================================
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
            Expression body;

            if (prop.PropertyType == typeof(int?))
                body = Expression.Equal(member, Expression.Convert(constant, typeof(int?)));
            else
                body = Expression.Equal(member, constant);

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        private static Task<T?> FirstOrDefaultByLongPropAsync<T>(IQueryable<T> query, System.Reflection.PropertyInfo prop, long value, CancellationToken ct)
            where T : class
        {
            var param = Expression.Parameter(typeof(T), "e");
            var member = Expression.Property(param, prop);

            Expression constant = Expression.Constant(value, typeof(long));
            Expression body;

            if (prop.PropertyType == typeof(long?))
                body = Expression.Equal(member, Expression.Convert(constant, typeof(long?)));
            else
                body = Expression.Equal(member, constant);

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        private static Task<T?> FirstOrDefaultByGuidPropAsync<T>(IQueryable<T> query, System.Reflection.PropertyInfo prop, Guid value, CancellationToken ct)
            where T : class
        {
            var param = Expression.Parameter(typeof(T), "e");
            var member = Expression.Property(param, prop);

            Expression constant = Expression.Constant(value, typeof(Guid));
            Expression body;

            if (prop.PropertyType == typeof(Guid?))
                body = Expression.Equal(member, Expression.Convert(constant, typeof(Guid?)));
            else
                body = Expression.Equal(member, constant);

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.FirstOrDefaultAsync(lambda, ct);
        }

        // =========================================================
        // Reflection helpers: read string by candidate property names
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

            // 再做一次忽略大小写匹配（防止属性名大小写不同）
            foreach (var name in candidates)
            {
                var p = type.GetProperties()
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (p != null) yield return p;
            }
        }

        private static string? InferPalmSideFromPalmId(string? palmId)
        {
            // 你之前 Palm 规则：60001=Left, 60002=Right
            if (string.IsNullOrWhiteSpace(palmId)) return null;
            var s = palmId.Trim().ToUpperInvariant();
            if (s.EndsWith("60001", StringComparison.Ordinal)) return "L";
            if (s.EndsWith("60002", StringComparison.Ordinal)) return "R";
            return null;
        }

        // =========================================================
        // SimplePartInfo constructors
        // =========================================================
        private static SimplePartInfo New(string raw, SimplePartKind kind, string kindName, string result, Dictionary<string, string> fields)
            => new(raw, kind, kindName, result, fields);

        private static SimplePartInfo Unknown(string raw, string reason)
            => new(raw, SimplePartKind.Unknown, "Unknown", "Unknown",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["reason"] = reason });
    }
}
