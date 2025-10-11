using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using RestSharp;

namespace DexRobotPDA.Services;

public class ProcessThreeService : BaseService
{
    public ProcessThreeService(
        RestClient restClient,
        ILogger<AuthService> logger,ILocalStorageService localStorage) 
        : base(restClient, logger, localStorage)
    {
    }
    
    public async Task<List<PalmDto>?> GetPalmList(string taskId)
    {
        var request = new RestRequest("api/Palm/GetPalmList");
        request.AddParameter("taskId", taskId);
        return await ExecuteRequest<List<PalmDto>>(request);
    }
    
    public async Task<List<string>?> GetPalmDetail(string palm_id)
    {
        var request = new RestRequest("api/Palm/GetPalmDetail");
        request.AddParameter("palm_id", palm_id);
        return await ExecuteRequest<List<string>>(request);
    }
    

    public async Task<ApiResponse> AddPalm(AddPalmDto palmDto)
    {
        var request = new RestRequest("api/Palm/AddPalm", Method.Post);

        // 使用JSON格式发送手掌数据
        request.AddJsonBody(palmDto);

        _logger.LogInformation("尝试新增手掌 - 手掌ID: {PalmId}", palmDto.palm_id);
        var apiResponse = await ExecuteCommand(request);

        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);

        // 新增成功时处理返回数据
        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            var palmData = JsonSerializer.Serialize(apiResponse.ResultData);
            // 解析响应数据中的palm对象
            using (var doc = JsonDocument.Parse(palmData))
            {
                var palmJson = doc.RootElement.GetProperty("palm").GetRawText();
                var createdPalm = JsonSerializer.Deserialize<PalmDto>(palmJson);
                _logger.LogInformation("手掌新增成功 - 手掌ID: {PalmId}", createdPalm?.palm_id);
            }
        }
        else
        {
            _logger.LogWarning("手掌新增失败 - 手掌ID: {PalmId}, 错误信息: {Msg}", 
                palmDto.palm_id, apiResponse.Msg);
        }

        return apiResponse;
    }

    public async Task<ApiResponse> FingerBindPalm(string finger_id, string palm_id)
    {
        var request = new RestRequest("api/Finger/FingerBindPalm", Method.Post);
        request.AddJsonBody(new { 
            finger_id = finger_id, 
            palm_id = palm_id
        });
        
        var apiResponse = await ExecuteCommand(request);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        return apiResponse;
    }
    
    public async Task<ApiResponse> SplitBindPalm(string split_id, string palm_id)
    {
        var request = new RestRequest("api/Split/SplitBindPalm", Method.Post);
        request.AddJsonBody(new { 
            split_id = split_id, 
            palm_id = palm_id
        });
        
        var apiResponse = await ExecuteCommand(request);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        return apiResponse;
    }
    
    public async Task<ApiResponse> UnBindPalm(string palm_id)
    {

        var request = new RestRequest($"api/Palm/UnBindPalm?palm_id={palm_id}", Method.Put);

        _logger.LogInformation("尝试解绑手掌外壳 - 手掌外壳ID: {PalmId}", palm_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("手掌外壳解绑成功 - 手掌外壳ID: {PalmId}", palm_id);
        }
        else
        {
            _logger.LogWarning("手掌外壳解绑失败 - 手掌外壳ID: {PalmId}, 错误信息: {Msg}", palm_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    
    public async Task<ApiResponse> UnBindSplit(string split_id)
    {

        var request = new RestRequest($"api/Split/UnBindSplit?split_id={split_id}", Method.Put);

        _logger.LogInformation("尝试解绑分指机构外壳 - 分指机构外壳ID: {SplitId}", split_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("分指机构外壳解绑成功 - 分指机构外壳ID: {SplitId}", split_id);
        }
        else
        {
            _logger.LogWarning("分指机构外壳解绑失败 - 分指机构外壳ID: {SplitId}, 错误信息: {Msg}", split_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    
    public async Task<ApiResponse> ReBindSplit(string split_id, string task_id, string palm_id)
    {
        var request = new RestRequest("api/Split/ReBindSplit", Method.Put);
        var dto = new ReBindDto()
        {
            part_id = split_id,
            task_id = task_id,
            on_part_id = palm_id
        };
        request.AddJsonBody(dto);

        _logger.LogInformation("尝试重绑分指机构外壳 - 分指机构外壳ID: {SplitId}", split_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("分指机构外壳重绑成功 - 分指机构外壳ID: {SplitId}", split_id);
        }
        else
        {
            _logger.LogWarning("分指机构外壳重绑失败 - 分指机构外壳ID: {SplitId}, 错误信息: {Msg}", split_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    
    public async Task<ApiResponse> ReBindPalm(string palm_id, string task_id)
    {
        var request = new RestRequest("api/Palm/ReBindPalm", Method.Put);
        var dto = new ReBindDto()
        {
            part_id = palm_id,
            task_id = task_id,
        };
        request.AddJsonBody(dto);

        _logger.LogInformation("尝试重绑手掌外壳 - 手掌外壳ID: {SplitId}", palm_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("手掌外壳重绑成功 - 手掌外壳ID: {SplitId}", palm_id);
        }
        else
        {
            _logger.LogWarning("手掌外壳重绑失败 - 手掌外壳ID: {SplitId}, 错误信息: {Msg}", palm_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    
    
    public async Task<ApiResponse> UpdateSplit(SplitDto dto)
    {
        var request = new RestRequest("api/Split/UpdateSplit", Method.Put); // 修改端点
        request.AddJsonBody(dto);
        _logger.LogInformation("尝试更新分指机构 - 分指机构ID: {MotorId}", dto.split_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("分指机构更新成功 - 分指机构ID: {MotorId}", dto.split_id);
        }
        else
        {
            _logger.LogWarning("分指机构更新失败 - 分指机构ID: {MotorId}, 错误信息: {Msg}", dto.split_id, apiResponse.Msg);
        }
        return apiResponse;
    }
    
    public async Task<ApiResponse> UpdatePalm(PalmDto dto)
    {
        var request = new RestRequest("api/Palm/UpdatePalm", Method.Put); // 修改端点
        request.AddJsonBody(dto);
        _logger.LogInformation("尝试更新手掌外壳 - 手掌外壳ID: {MotorId}", dto.palm_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("手掌外壳更新成功 - 手掌外壳ID: {MotorId}", dto.palm_id);
        }
        else
        {
            _logger.LogWarning("手掌外壳更新失败 - 手掌外壳ID: {MotorId}, 错误信息: {Msg}", dto.palm_id, apiResponse.Msg);
        }
        return apiResponse;
    }
    
    public async Task<ApiResponse> AddPalmWithComponents(AddPalmWithComponentsDto dto)
    {
        var request = new RestRequest("api/Palm/AddPalmWithComponents", Method.Post);
        request.AddJsonBody(dto);
    
        _logger.LogInformation("尝试创建手掌并绑定组件 - 手掌ID: {PalmId}", dto.palm_id);
        var apiResponse = await ExecuteCommand(request);
    
        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("手掌创建并绑定组件成功 - 手掌ID: {PalmId}", dto.palm_id);
        }
        else
        {
            _logger.LogWarning("手掌创建并绑定组件失败 - 手掌ID: {PalmId}, 错误信息: {Msg}", 
                dto.palm_id, apiResponse.Msg);
        }
    
        return apiResponse;
    }

}