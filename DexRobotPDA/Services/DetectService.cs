using System.Text.Json;
using Blazored.LocalStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace DexRobotPDA.Services;

public class DetectService : BaseService
{
    private readonly ILogger<DetectService> _logger;

    public DetectService(
        RestClient restClient,
        ILogger<DetectService> logger,
        ILocalStorageService localStorage)
        : base(restClient, logger, localStorage)
    {
        _logger = logger;
    }


    public async Task<ApiResponse> AddDetect1(UpdateDetect1Dto detect1Dto)
    {
        try
        {
            // 发送创建检测记录的请求
            var detectRequest = new RestRequest("api/Detect1/AddDetect1", Method.Post);
            detectRequest.AddJsonBody(detect1Dto);

            _logger.LogInformation("开始为电机 {MotorId} 创建检测记录", detect1Dto.motor_id);
            var detectResponse = await ExecuteCommand(detectRequest);

            if (detectResponse.ResultCode == 1)
            {
                _logger.LogInformation("电机 {MotorId} 的检测记录创建成功", detect1Dto.motor_id);
            }
            else
            {
                _logger.LogWarning("电机 {MotorId} 的检测记录创建失败: {Message}", detect1Dto.motor_id, detectResponse.Msg);
            }
            return detectResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建电机 {MotorId} 的检测记录时发生异常", detect1Dto.motor_id);
            throw;
        }
    }
    
    public async Task<ApiResponse> AddDetect2(SplitWormDetectCreateDto detect1Dto)
    {
        try
        {
            // 发送创建检测记录的请求
            var detectRequest = new RestRequest("api/Detect2/AddDetect2", Method.Post);
            detectRequest.AddJsonBody(detect1Dto);

            _logger.LogInformation("开始为分指机构 {MotorId} 创建检测记录", detect1Dto.split_id);
            var detectResponse = await ExecuteCommand(detectRequest);

            if (detectResponse.ResultCode == 1)
            {
                _logger.LogInformation("分指机构 {MotorId} 的检测记录创建成功", detect1Dto.split_id);
            }
            else
            {
                _logger.LogWarning("分指机构 {MotorId} 的检测记录创建失败: {Message}", detect1Dto.split_id, detectResponse.Msg);
            }
            return detectResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建分指机构 {MotorId} 的检测记录时发生异常", detect1Dto.split_id);
            throw;
        }
    }
    public async Task<ApiResponse> AddDetect3(SplitCalibrateDetectCreateDto detect1Dto)
    {
        try
        {
            // 发送创建检测记录的请求
            var detectRequest = new RestRequest("api/Detect3/AddDetect3", Method.Post);
            detectRequest.AddJsonBody(detect1Dto);

            _logger.LogInformation("开始为分指机构 {MotorId} 创建检测记录", detect1Dto.split_id);
            var detectResponse = await ExecuteCommand(detectRequest);

            if (detectResponse.ResultCode == 1)
            {
                _logger.LogInformation("分指机构 {MotorId} 的检测记录创建成功", detect1Dto.split_id);
            }
            else
            {
                _logger.LogWarning("分指机构 {MotorId} 的检测记录创建失败: {Message}", detect1Dto.split_id, detectResponse.Msg);
            }
            return detectResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建分指机构 {MotorId} 的检测记录时发生异常", detect1Dto.split_id);
            throw;
        }
    }

    public async Task<ApiResponse> AddDetect4(FingerCalibrateDetectCreateDto detect1Dto)
    {
        try
        {
            // 发送创建检测记录的请求
            var detectRequest = new RestRequest("api/Detect4/AddDetect4", Method.Post);
            detectRequest.AddJsonBody(detect1Dto);

            _logger.LogInformation("开始为手指 {MotorId} 创建检测记录", detect1Dto.finger_id);
            var detectResponse = await ExecuteCommand(detectRequest);

            if (detectResponse.ResultCode == 1)
            {
                _logger.LogInformation("手指 {MotorId} 的检测记录创建成功", detect1Dto.finger_id);
            }
            else
            {
                _logger.LogWarning("手指 {MotorId} 的检测记录创建失败: {Message}", detect1Dto.finger_id, detectResponse.Msg);
            }
            return detectResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建手指 {MotorId} 的检测记录时发生异常", detect1Dto.finger_id);
            throw;
        }
    }
    
    public async Task<ApiResponse> AddDetect5(PalmCalibrateDetectCreateDto detect1Dto)
    {
        try
        {
            // 输出请求内容
            var options = new JsonSerializerOptions { WriteIndented = true };
            string requestJson = JsonSerializer.Serialize(detect1Dto, options);
            
            // 发送创建检测记录的请求
            var detectRequest = new RestRequest("api/Detect5/AddDetect5", Method.Post);
            detectRequest.AddJsonBody(detect1Dto);

            _logger.LogInformation("开始为手掌 {MotorId} 创建检测记录", detect1Dto.palm_id);
            var detectResponse = await ExecuteCommand(detectRequest);

            if (detectResponse.ResultCode == 1)
            {
                _logger.LogInformation("手掌 {MotorId} 的检测记录创建成功", detect1Dto.palm_id);
            }
            else
            {
                _logger.LogWarning("手掌 {MotorId} 的检测记录创建失败: {Message}", detect1Dto.palm_id, detectResponse.Msg);
            }
        
            // 输出响应内容
            string responseJson = JsonSerializer.Serialize(detectResponse, options);


            return detectResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建手掌 {MotorId} 的检测记录时发生异常", detect1Dto.palm_id);
            throw;
        }
    }

    
    public async Task<MotorDto> GetMotor(string motor_id)
    {
        var request = new RestRequest("api/Motor/GetMotor");
        request.AddParameter("motor_id", motor_id);
        return await ExecuteRequest<MotorDto>(request);
    }

    public async Task<SplitDto> GetSplit(string split_id)
    {
        var request = new RestRequest("api/Split/GetSplit");
        request.AddParameter("split_id", split_id);
        return await ExecuteRequest<SplitDto>(request);
    }
    
    public async Task<FingerDto> GetFinger(string finger_id)
    {
        var request = new RestRequest("api/Finger/GetFinger");
        request.AddParameter("finger_id", finger_id);
        return await ExecuteRequest<FingerDto>(request);
    }
    
    public async Task<PalmDto> GetPalm(string palm_id)
    {
        var request = new RestRequest("api/Palm/GetPalm");
        request.AddParameter("palm_id", palm_id);
        return await ExecuteRequest<PalmDto>(request);
    }

    public async Task<ApiResponse> UpdateDetect1(MotorWormDetectDto detect1Dto)
    {
        var request = new RestRequest("api/Detect1/UpdateLatestDetect", Method.Put);

        // 先输出请求内容
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(detect1Dto, options);


        // 使用JSON格式发送检测数据
        request.AddJsonBody(detect1Dto);

        _logger.LogInformation("尝试更新检测记录 - 电机ID: {MotorId}", detect1Dto.motor_id);

        try
        {
            var apiResponse = await ExecuteCommand(request);

            // 序列化并打印响应
            string responseJson = JsonSerializer.Serialize(apiResponse, options);


            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 电机ID: {MotorId}", detect1Dto.motor_id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 电机ID: {MotorId}, 错误信息: {Msg}",
                    detect1Dto.motor_id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<MotorWormDetectDto?> GetMotorWormDetect(string motor_id)
    {
        var request = new RestRequest("api/Detect1/GetMotorWormDetect");
        request.AddParameter("motor_id", motor_id);
        return await ExecuteRequest<MotorWormDetectDto>(request);
    }

    public async Task<List<MotorWormDetectDto>?> GetMotorWormDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect1/GetMotorWormDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<MotorWormDetectDto>>(request);
    }

    public async Task<List<SplitWormDetectDto>?> GetSplitWormDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect2/GetSplitWormDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<SplitWormDetectDto>>(request);
    }
    
    public async Task<List<SplitCalibrateDetectDto>?> GetSplitCalibrateDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect3/GetSplitCalibrateDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<SplitCalibrateDetectDto>>(request);
    }
    
    public async Task<List<FingerCalibrateDetectDto>?> GetFingerCalibrateDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect4/GetFingerCalibrateDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<FingerCalibrateDetectDto>>(request);
    }
    
    public async Task<List<PalmCalibrateDetectDto>?> GetPalmCalibrateDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect5/GetPalmCalibrateDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<PalmCalibrateDetectDto>>(request);
    }

    public async Task<ApiResponse> UpdateMotorQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Motor/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateFingerQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Finger/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }
    public async Task<ApiResponse> UpdatePalmQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Palm/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateSplitQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Split/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateDetect1StatusAsync(string taskId)
    {
        try
        {
            var request = new RestRequest("api/ProductTask/UpdateDetect1Status", Method.Put);
        
            // 明确指定为查询字符串参数
            request.AddQueryParameter("taskId", taskId);
        
            // 或者使用 URL 参数方式
            // request = new RestRequest($"api/ProductTask/UpdateDetect1Status?taskId={Uri.EscapeDataString(taskId)}", Method.Put);

            _logger.LogInformation("开始更新任务 {TaskId} 的蜗杆粘接检测状态", taskId);

            var response = await ExecuteCommand(request);

            if (response.ResultCode == 1)
            {
                _logger.LogInformation("任务 {TaskId} 的蜗杆粘接检测状态更新成功，合格数量: {Count}", 
                    taskId, response.Msg);
            }
            else
            {
                _logger.LogWarning("任务 {TaskId} 的蜗杆粘接检测状态更新失败: {Message}", 
                    taskId, response.Msg);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任务 {TaskId} 的蜗杆粘接检测状态时发生异常", taskId);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateDetect2StatusAsync(string taskId)
    {
        try
        {
            var request = new RestRequest("api/ProductTask/UpdateDetect2Status", Method.Put);
        
            // 明确指定为查询字符串参数
            request.AddQueryParameter("taskId", taskId);
        
            // 或者使用 URL 参数方式
            // request = new RestRequest($"api/ProductTask/UpdateDetect1Status?taskId={Uri.EscapeDataString(taskId)}", Method.Put);

            _logger.LogInformation("开始更新任务 {TaskId} 的蜗杆粘接检测状态", taskId);

            var response = await ExecuteCommand(request);

            if (response.ResultCode == 1)
            {
                _logger.LogInformation("任务 {TaskId} 的蜗杆粘接检测状态更新成功，合格数量: {Count}", 
                    taskId, response.Msg);
            }
            else
            {
                _logger.LogWarning("任务 {TaskId} 的蜗杆粘接检测状态更新失败: {Message}", 
                    taskId, response.Msg);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任务 {TaskId} 的蜗杆粘接检测状态时发生异常", taskId);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateDetect3StatusAsync(string taskId)
    {
        try
        {
            var request = new RestRequest("api/ProductTask/UpdateDetect3Status", Method.Put);
        
            // 明确指定为查询字符串参数
            request.AddQueryParameter("taskId", taskId);
        
            // 或者使用 URL 参数方式
            // request = new RestRequest($"api/ProductTask/UpdateDetect1Status?taskId={Uri.EscapeDataString(taskId)}", Method.Put);

            _logger.LogInformation("开始更新任务 {TaskId} 的蜗杆粘接检测状态", taskId);

            var response = await ExecuteCommand(request);

            if (response.ResultCode == 1)
            {
                _logger.LogInformation("任务 {TaskId} 的蜗杆粘接检测状态更新成功，合格数量: {Count}", 
                    taskId, response.Msg);
            }
            else
            {
                _logger.LogWarning("任务 {TaskId} 的蜗杆粘接检测状态更新失败: {Message}", 
                    taskId, response.Msg);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任务 {TaskId} 的蜗杆粘接检测状态时发生异常", taskId);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateDetect4StatusAsync(string taskId)
    {
        try
        {
            var request = new RestRequest("api/ProductTask/UpdateDetect4Status", Method.Put);
        
            // 明确指定为查询字符串参数
            request.AddQueryParameter("taskId", taskId);
        
            // 或者使用 URL 参数方式
            // request = new RestRequest($"api/ProductTask/UpdateDetect1Status?taskId={Uri.EscapeDataString(taskId)}", Method.Put);

            _logger.LogInformation("开始更新任务 {TaskId} 的蜗杆粘接检测状态", taskId);

            var response = await ExecuteCommand(request);

            if (response.ResultCode == 1)
            {
                _logger.LogInformation("任务 {TaskId} 的蜗杆粘接检测状态更新成功，合格数量: {Count}", 
                    taskId, response.Msg);
            }
            else
            {
                _logger.LogWarning("任务 {TaskId} 的蜗杆粘接检测状态更新失败: {Message}", 
                    taskId, response.Msg);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任务 {TaskId} 的蜗杆粘接检测状态时发生异常", taskId);
            throw;
        }
    }
    public async Task<ApiResponse> UpdateDetect5StatusAsync(string taskId)
    {
        try
        {
            var request = new RestRequest("api/ProductTask/UpdateDetect5Status", Method.Put);
        
            // 明确指定为查询字符串参数
            request.AddQueryParameter("taskId", taskId);

            _logger.LogInformation("开始更新任务 {TaskId} 的蜗杆粘接检测状态", taskId);

            var response = await ExecuteCommand(request);

            if (response.ResultCode == 1)
            {
                _logger.LogInformation("任务 {TaskId} 的蜗杆粘接检测状态更新成功，合格数量: {Count}", 
                    taskId, response.Msg);
            }
            else
            {
                _logger.LogWarning("任务 {TaskId} 的蜗杆粘接检测状态更新失败: {Message}", 
                    taskId, response.Msg);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任务 {TaskId} 的蜗杆粘接检测状态时发生异常", taskId);
            throw;
        }
    }
    
}