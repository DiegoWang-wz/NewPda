using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using RestSharp;
using DexRobotPDA.DTOs;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using Microsoft.Extensions.Logging;

namespace DexRobotPDA.Services
{
    public class TaskService : BaseService
    {
        public TaskService(RestClient restClient, ILogger<EmployeeService> logger, ILocalStorageService localStorage)
            : base(restClient, logger, localStorage)
        {
        }

        public async Task<List<ProductTaskDto>?> GetTasks()
        {
            var request = new RestRequest("api/ProductTask/GetTaskList");
            return await ExecuteRequest<List<ProductTaskDto>>(request);
        }

        public async Task<ProductTaskDto?> GetTaskDetail(string taskId)
        {
            var request = new RestRequest("api/ProductTask/GetTaskDetail");
            request.AddParameter("taskId", taskId);
            return await ExecuteRequest<ProductTaskDto>(request);
        }

        public async Task<ApiResponse> AddTask(AddTaskDto taskDto)
        {
            var request = new RestRequest("api/ProductTask/AddTask", Method.Post);

            request.AddJsonBody(taskDto);
            var apiResponse = await ExecuteCommand(request);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
            {
                _logger.LogInformation("任务新增成功 - 任务标题: {title}", taskDto.title);
            }
            else
            {
                _logger.LogWarning("任务新增失败 - 任务标题: {title}, 错误信息: {Msg}", taskDto.title, apiResponse.Msg);
            }

            return apiResponse;
        }


        public async Task<ApiResponse> UpdateTaskProcessStatus(string taskId, string process, byte status)
        {
            // 控制台输出传入的参数

            var request = new RestRequest("api/ProductTask/UpdateTaskProcessStatus", Method.Post);
            // 传递JSON体参数，与后端DTO匹配
            request.AddJsonBody(new
            {
                task_id = taskId,
                process = process,
                status = status
            });

            _logger.LogInformation("尝试更新任务流程状态 - 任务ID: {TaskId}, 流程: {Process}, 目标状态: {Status}",
                taskId, process, status);

            var apiResponse = await ExecuteCommand(request);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
            {
                var taskData = JsonSerializer.Serialize(apiResponse.ResultData);
                var updatedTask = JsonSerializer.Deserialize<ProductTaskModel>(taskData);
                _logger.LogInformation("任务流程状态更新成功 - 任务ID: {TaskId}, 流程: {Process}, 当前状态: {Status}",
                    updatedTask?.task_id, process, status);
            }
            else
            {
                _logger.LogWarning("任务流程状态更新失败 - 任务ID: {TaskId}, 流程: {Process}, 错误信息: {Msg}",
                    taskId, process, apiResponse.Msg);
            }

            return apiResponse;
        }

        public async Task<ApiResponse> UpdateTaskStatus(string taskId, byte status)
        {
            // 1. 构建POST请求（后端API为POST，参数通过QueryString传递）
            var request = new RestRequest("api/ProductTask/UpdateTaskStatus", Method.Post);
            request.AddJsonBody(new
            {
                task_id = taskId,
                status = status
            });
            _logger.LogInformation("尝试更新任务整体状态 - 任务ID: {TaskId}, 目标状态: {Status}",
                taskId, status);

            // 2. 执行请求
            var apiResponse = await ExecuteCommand(request);

            // 3. 序列化并打印响应
            var options = new JsonSerializerOptions { WriteIndented = true };
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            // 4. 处理响应结果
            if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
            {
                // 反序列化ResultData为任务模型（根据实际返回调整为Dto或Model）
                var taskData = JsonSerializer.Serialize(apiResponse.ResultData);
                var updatedTask = JsonSerializer.Deserialize<ProductTaskModel>(taskData);
                _logger.LogInformation("任务整体状态更新成功 - 任务ID: {TaskId}, 当前状态: {Status}",
                    updatedTask?.task_id, status);
            }
            else
            {
                _logger.LogWarning("任务整体状态更新失败 - 任务ID: {TaskId}, 错误信息: {Msg}",
                    taskId, apiResponse.Msg);
            }

            return apiResponse;
        }

        public async Task<ApiResponse> UpdateAllTaskStatus()
        {
            var request = new RestRequest("api/ProductTask/UpdateAllTaskStatus", Method.Put);

            _logger.LogInformation("尝试批量更新所有任务状态");

            var apiResponse = await ExecuteCommand(request);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
            {
                _logger.LogInformation("批量更新任务状态成功");
            }
            else
            {
                _logger.LogWarning("批量更新任务状态失败 - 错误信息: {Msg}", apiResponse.Msg);
            }

            return apiResponse;
        }

        public async Task<ApiResponse> UpdateSingleTaskStatus(string taskId)
        {
            var request = new RestRequest("api/ProductTask/UpdateSingleTaskStatus", Method.Put);
            request.AddParameter("taskId", taskId);

            _logger.LogInformation("尝试更新单个任务状态 - 任务ID: {TaskId}", taskId);

            var apiResponse = await ExecuteCommand(request);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string responseJson = JsonSerializer.Serialize(apiResponse, options);

            if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
            {
                _logger.LogInformation("单个任务状态更新成功 - 任务ID: {TaskId}", taskId);
            }
            else
            {
                _logger.LogWarning("单个任务状态更新失败 - 任务ID: {TaskId}, 错误信息: {Msg}",
                    taskId, apiResponse.Msg);
            }

            return apiResponse;
        }
        
        public async Task<List<FullTaskDataDto>?> GetFullTaskData(string taskId)
        {
            var request = new RestRequest("api/ProductTask/GetFullTaskData");
            request.AddParameter("taskId", taskId);
            return await ExecuteRequest<List<FullTaskDataDto>>(request);
        }


    }
}