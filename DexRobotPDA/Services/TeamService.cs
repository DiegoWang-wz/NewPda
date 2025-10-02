using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using RestSharp;
using DexRobotPDA.DTOs;
using DexRobotPDA.ApiResponses;
using Microsoft.Extensions.Logging;

namespace DexRobotPDA.Services
{
    public class TeamService : BaseService
    {
        public TeamService(RestClient restClient, ILogger<EmployeeService> logger,ILocalStorageService localStorage) 
            : base(restClient, logger, localStorage)
        {
        }

        public async Task<List<TeamDto>?> GetListAsync()
        {
            var request = new RestRequest("api/Team/GetTeams");
            return await ExecuteRequest<List<TeamDto>>(request);
        }

    }
}
    