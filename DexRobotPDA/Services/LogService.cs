using System.Text.Json;
using Blazored.LocalStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace DexRobotPDA.Services;

public class LogService : BaseService
{
    private readonly ILogger<LogService> _logger;

    public LogService(
        RestClient restClient,
        ILogger<LogService> logger,
        ILocalStorageService localStorage)
        : base(restClient, logger, localStorage)
    {
        _logger = logger;
    }

    public async Task<List<EventLogDto>?> GetLogs(DateTime? startTime = null, DateTime? endTime = null)
    {
        var request = new RestRequest("api/EventLog/GetEventLogs", Method.Get);
        if (startTime.HasValue)
            request.AddParameter("startDate", startTime.Value.ToString("yyyy-MM-dd"));
        if (endTime.HasValue)
            request.AddParameter("endDate", endTime.Value.ToString("yyyy-MM-dd"));

        return await ExecuteRequest<List<EventLogDto>>(request);
    }


    public async Task<ApiResponse> AddLog(string event_type, string operator_id, string event_detail, bool is_qualified)
    {
        var request = new RestRequest("api/EventLog/AddEventLog", Method.Post);
        AddEventLogDto logDto = new AddEventLogDto()
        {
            // 设置各个属性值
            event_type = event_type,
            operator_id = operator_id,
            event_detail = event_detail,
            operate_time = DateTime.Now,
            is_qualified = is_qualified
        };
        request.AddJsonBody(logDto);
        var apiResponse = await ExecuteCommand(request);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            _logger.LogInformation("日志新增成功 - 日志类型: {title}", logDto.event_type);
        }
        else
        {
            _logger.LogWarning("日志新增失败 - 日志类型: {title}, 错误信息: {Msg}", logDto.event_type, apiResponse.Msg);
        }

        return apiResponse;
    }
}