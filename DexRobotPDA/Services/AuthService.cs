using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using DexRobotPDA.DTOs;
using DexRobotPDA.ApiResponses;

namespace DexRobotPDA.Services
{
    public class AuthService : BaseService
    {
        // 构造函数注入 ILocalStorageService
        public AuthService(
            RestClient restClient, 
            ILogger<AuthService> logger, 
            ILocalStorageService localStorage) 
            : base(restClient, logger, localStorage)
        {
        }

        public async Task<ApiResponse> Login(string employee_id, string password)
        {
            var request = new RestRequest("api/Auth/Login", Method.Post);
    
            // 使用JSON格式发送数据
            request.AddJsonBody(new 
            { 
                employee_id = employee_id,
                password = password 
            });
    
            _logger.LogInformation("尝试登录 - 员工ID: {EmployeeId}", employee_id);
            var apiResponse = await ExecuteCommand(request);
    
            // 序列化并打印响应
            var options = new JsonSerializerOptions { WriteIndented = true };
            string responseJson = JsonSerializer.Serialize(apiResponse, options);
            Console.WriteLine("API响应内容:");
            Console.WriteLine(responseJson);

            // 登录成功时存储用户信息到LocalStorage
            if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
            {
                var userData = JsonSerializer.Serialize(apiResponse.ResultData);
                var userDto = JsonSerializer.Deserialize<UserDto>(userData);
                await SetLocalDataAsync("CurrentUser", userDto);
                _logger.LogInformation("登录成功并存储用户信息 - 员工ID: {EmployeeId}", employee_id);
            }

            return apiResponse;
        }
        
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                return await GetLocalDataAsync<UserDto>("CurrentUser");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前用户信息失败");
                return null;
            }
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }

        public async Task LogoutAsync()
        {
            await RemoveLocalDataAsync("CurrentUser");
            _logger.LogInformation("用户已退出登录");
        }
    }
}
