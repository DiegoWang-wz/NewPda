using Blazored.LocalStorage;
using RestSharp;
using DexRobotPDA.DTOs;


namespace DexRobotPDA.Services
{
    public class EmployeeService : BaseService
    {
        public EmployeeService(RestClient restClient, ILogger<EmployeeService> logger,ILocalStorageService localStorage) 
            : base(restClient, logger, localStorage)
        {
        }
        
        public async Task<List<EmployeeDto>?> GetEmployee()
        {
            var request = new RestRequest("api/Employee/GetEmployee");
            return await ExecuteRequest<List<EmployeeDto>>(request);
        }

    }
}
    