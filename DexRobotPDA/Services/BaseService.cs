using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using DexRobotPDA.ApiResponses;
using Microsoft.Extensions.Logging;
// 引入 Blazored.LocalStorage 命名空间
using Blazored.LocalStorage;

namespace DexRobotPDA.Services
{
    public abstract class BaseService
    {
        protected readonly RestClient _restClient;
        protected readonly ILogger _logger;
        protected readonly JsonSerializerOptions _jsonOptions;
        // 使用 LocalStorage 服务
        protected readonly ILocalStorageService _localStorage;

        // 构造函数注入 ILocalStorageService
        protected BaseService(
            RestClient restClient, 
            ILogger logger, 
            ILocalStorageService localStorage)
        {
            _restClient = restClient;
            _logger = logger;
            _localStorage = localStorage;
            
            // 全局JSON序列化选项 - 大小写不敏感
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        #region 封装 LocalStorage 通用操作方法
        /// <summary>
        /// 存储数据到 LocalStorage
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">存储键</param>
        /// <param name="value">要存储的值</param>
        protected async Task SetLocalDataAsync<T>(string key, T value)
        {
            try
            {
                _logger.LogDebug("存储LocalStorage数据 - 键: {Key}, 类型: {TypeName}", key, typeof(T).Name);
                await _localStorage.SetItemAsync(key, value);
                _logger.LogInformation("LocalStorage数据存储成功 - 键: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "存储LocalStorage数据失败 - 键: {Key}", key);
                throw;
            }
        }

        /// <summary>
        /// 从 LocalStorage 读取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">存储键</param>
        /// <returns>读取到的数据</returns>
        protected async Task<T?> GetLocalDataAsync<T>(string key)
        {
            try
            {
                _logger.LogDebug("读取LocalStorage数据 - 键: {Key}, 类型: {TypeName}", key, typeof(T).Name);
                var data = await _localStorage.GetItemAsync<T>(key);
                
                if (data == null)
                {
                    _logger.LogWarning("LocalStorage数据不存在 - 键: {Key}", key);
                    return default;
                }
                
                _logger.LogInformation("LocalStorage数据读取成功 - 键: {Key}", key);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取LocalStorage数据失败 - 键: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// 从 LocalStorage 删除指定键的数据
        /// </summary>
        /// <param name="key">存储键</param>
        protected async Task RemoveLocalDataAsync(string key)
        {
            try
            {
                _logger.LogDebug("删除LocalStorage数据 - 键: {Key}", key);
                await _localStorage.RemoveItemAsync(key);
                _logger.LogInformation("LocalStorage数据删除成功 - 键: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除LocalStorage数据失败 - 键: {Key}", key);
                throw;
            }
        }

        /// <summary>
        /// 清空所有 LocalStorage 数据
        /// </summary>
        protected async Task ClearAllLocalDataAsync()
        {
            try
            {
                _logger.LogDebug("清空所有LocalStorage数据");
                await _localStorage.ClearAsync();
                _logger.LogInformation("所有LocalStorage数据清空成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空LocalStorage数据失败");
                throw;
            }
        }
        #endregion

        // API 请求方法（保持不变）
        protected async Task<T?> ExecuteRequest<T>(RestRequest request)
        {
            try
            {
                var fullUrl = _restClient.BuildUri(request).ToString();
                _logger.LogInformation("请求URL: {Url}", fullUrl);

                var response = await _restClient.ExecuteAsync(request);

                _logger.LogInformation("请求状态: {StatusCode}, 成功: {IsSuccessful}", 
                    response.StatusCode, response.IsSuccessful);

                if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                {
                    _logger.LogWarning("请求失败或内容为空: {ErrorMessage}", response.ErrorMessage);
                    return default;
                }

                // 先反序列化为ApiResponse
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response.Content, _jsonOptions);
                if (apiResponse == null)
                {
                    _logger.LogWarning("无法反序列化为ApiResponse");
                    return default;
                }

                _logger.LogInformation("API返回: ResultCode={ResultCode}, Msg={Msg}", 
                    apiResponse.ResultCode, apiResponse.Msg);

                // 处理ResultData
                if (apiResponse.ResultData != null)
                {
                    try
                    {
                        var resultDataJson = JsonSerializer.Serialize(apiResponse.ResultData, _jsonOptions);
                        return JsonSerializer.Deserialize<T>(resultDataJson, _jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ResultData反序列化失败");
                        return default;
                    }
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API请求执行失败");
                return default;
            }
        }

        // 命令执行方法（保持不变）
        protected async Task<ApiResponse> ExecuteCommand(RestRequest request)
        {
            try
            {
                var fullUrl = _restClient.BuildUri(request).ToString();
                _logger.LogInformation("执行命令URL: {Url}", fullUrl);

                var response = await _restClient.ExecuteAsync(request);

                if (string.IsNullOrEmpty(response.Content))
                {
                    _logger.LogWarning("命令响应内容为空");
                    return new ApiResponse { ResultCode = -99, Msg = "响应内容为空" };
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response.Content, _jsonOptions);
                return apiResponse ?? new ApiResponse { ResultCode = -99, Msg = "无法解析响应" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "命令执行失败");
                return new ApiResponse { ResultCode = -99, Msg = ex.Message };
            }
        }
    }
}
