using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using RestSharp;

namespace DexRobotPDA.Services;

public class ProcessTwoService : BaseService
{
    public ProcessTwoService(
        RestClient restClient,
        ILogger<AuthService> logger,ILocalStorageService localStorage) 
        : base(restClient, logger, localStorage)
    {
    }
    

    
    public async Task<List<FingerDto>?> GetFinishedList(string taskId)
    {
        var request = new RestRequest("api/Finger/GetFingerList");
        request.AddParameter("taskId", taskId);
        return await ExecuteRequest<List<FingerDto>>(request);
    }

    
    public async Task<List<string>?> GetFingerDetail(string finger_id)
    {
        var request = new RestRequest("api/Finger/GetFingerDetail");
        request.AddParameter("finger_id", finger_id);
        return await ExecuteRequest<List<string>>(request);
    }
    public async Task<QualifyDto> CheckMotor(string motor_id)
    {
        var request = new RestRequest("api/Motor/GetMotorQualify");
        request.AddParameter("motor_id", motor_id);
        var apiResponse = await ExecuteCommand(request);

        // 处理API调用失败的情况（转换为不合格的QualifyDto）
        if (apiResponse.ResultCode != 1 || apiResponse.ResultData == null)
        {
            return new QualifyDto
            {
                qualify = false,
                message = apiResponse.Msg ?? "获取电机状态失败"
            };
        }

        try
        {
            // 解析主接口返回的QualifyDto
            var qualifyDto = JsonSerializer.Deserialize<QualifyDto>(
                JsonSerializer.Serialize(apiResponse.ResultData)
            );

            // 如果电机不合格，调用详情接口获取具体原因
            if (qualifyDto != null && !qualifyDto.qualify)
            {
                var detailRequest = new RestRequest("api/Detect1/Detect1Message");
                detailRequest.AddParameter("motor_id", motor_id);
                var detailResponse = await ExecuteCommand(detailRequest);

                // 解析详情接口返回的QualifyDto
                if (detailResponse.ResultCode == 1 && detailResponse.ResultData != null)
                {
                    var detailDto = JsonSerializer.Deserialize<QualifyDto>(
                        JsonSerializer.Serialize(detailResponse.ResultData)
                    );
                    return detailDto ?? qualifyDto; // 优先返回详情接口数据
                }
            
                // 详情接口调用失败时，保留基础不合格信息
                qualifyDto.message = detailResponse.Msg ?? "获取不合格详情失败";
                return qualifyDto;
            }

            // 电机合格时直接返回主接口数据
            return qualifyDto ?? new QualifyDto { qualify = true, message = "电机合格" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析电机状态数据失败");
            // 解析失败时返回明确的错误信息
            return new QualifyDto
            {
                qualify = false,
                message = "解析电机状态数据失败：" + ex.Message
            };
        }
    }
    
    public async Task<ApiResponse> AddFinger(AddFingerDto fingerDto)
    {
        var request = new RestRequest("api/Finger/AddFinger", Method.Post);

        // 使用JSON格式发送手指数据
        request.AddJsonBody(fingerDto);

        _logger.LogInformation("尝试新增手指 - 手指ID: {FingerId}", fingerDto.finger_id);
        var apiResponse = await ExecuteCommand(request);

        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        // 新增成功时处理返回数据
        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            var fingerData = JsonSerializer.Serialize(apiResponse.ResultData);
            // 解析响应数据中的finger对象
            using (var doc = JsonDocument.Parse(fingerData))
            {
                var fingerJson = doc.RootElement.GetProperty("finger").GetRawText();
                var createdFinger = JsonSerializer.Deserialize<FingerDto>(fingerJson);
                _logger.LogInformation("手指新增成功 - 手指ID: {FingerId}", createdFinger?.finger_id);
            }
        }
        else
        {
            _logger.LogWarning("手指新增失败 - 手指ID: {FingerId}, 错误信息: {Msg}", 
                fingerDto.finger_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    public async Task<ApiResponse> MotorBindFinger(string motor_id, string finger_id)
    {
        var request = new RestRequest("api/Motor/MotorBindFinger", Method.Post);
        request.AddJsonBody(new { 
            motor_id = motor_id, 
            finger_id = finger_id
        });
        
        var apiResponse = await ExecuteCommand(request);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("更新任务流程状态API响应内容:");
        Console.WriteLine(responseJson);
        return apiResponse;
    }
    
    public async Task<ApiResponse> UnBindFinger(string finger_id)
    {

        var request = new RestRequest($"api/Finger/UnBindFinger?finger_id={finger_id}", Method.Put);

        _logger.LogInformation("尝试解绑手指外壳 - 手指外壳ID: {FingerId}", finger_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("手指外壳解绑成功 - 手指外壳ID: {FingerId}", finger_id);
        }
        else
        {
            _logger.LogWarning("手指外壳解绑失败 - 手指外壳ID: {FingerId}, 错误信息: {Msg}", finger_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    
    public async Task<ApiResponse> ReBindFinger(string finger_id, string task_id, string palm_id)
    {
        var request = new RestRequest("api/Finger/ReBindFinger", Method.Put); // 修改端点
        var dto = new ReBindDto()
        {
            part_id = finger_id,
            task_id = task_id,
            on_part_id = palm_id
        };
        request.AddJsonBody(dto);

        _logger.LogInformation("尝试重绑手指外壳 - 手指外壳ID: {FingerId}", finger_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("手指外壳重绑成功 - 手指外壳ID: {FingerId}", finger_id);
        }
        else
        {
            _logger.LogWarning("手指外壳重绑失败 - 手指外壳ID: {FingerId}, 错误信息: {Msg}", finger_id, apiResponse.Msg);
        }

        return apiResponse;
    }
}