using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using DexRobotPDA.Services;
using Microsoft.AspNetCore.Mvc;

namespace DexRobotPDA.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TasksController : ControllerBase
    {
        private readonly IDX023Service _idx023Service;
        private readonly ITasksService _itaskService;
        private readonly ILogger<ServoController> _logger;

        public TasksController(IDX023Service idx023Service, ITasksService itaskService, ILogger<ServoController> logger)
        {
            _idx023Service = idx023Service;
            _itaskService = itaskService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFullTaskData(string taskId, CancellationToken ct)
        {
            var res = await _itaskService.GetFullTaskDataAsync(taskId, ct);
            return Ok(res);
        }

        [HttpGet]
        public Task<ApiResponse<bool>> UpdateTaskProcessStatus(
            string task_id,
            int process,
            bool status = false,
            CancellationToken ct = default) => _itaskService.UpdateTaskProcessStatus(task_id, process, status, ct);

        [HttpGet]
        public Task<ApiResponse<ProductTaskDto>> GetTaskById(string taskId, CancellationToken ct = default) =>
            _itaskService.GetTaskById(taskId, ct);
    }
}