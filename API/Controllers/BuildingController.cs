using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _buildingService;
        public BuildingController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }
        [HttpGet("registration")]
        public async Task<IActionResult> GetAllBuildings()
        {
            var (success, message, statusCode, buildings) = await _buildingService.GetAllBuildingAsync();
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message,
                    data = buildings
                });
            }
            return StatusCode(statusCode, new
            {
                success = false,
                message = message
            });
        }

        [HttpGet("manager/{buildingId}")]
        public async Task<IActionResult> GetBuildingWithManager(string buildingId)
        {
            var (success, message, statusCode, data) = await _buildingService.GetBuildingWithManagerAsync(buildingId);
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message,
                    data = data
                });
            }
            return StatusCode(statusCode, new
            {
                success = false,
                message = message
            });
        }

        [HttpGet("manager/rooms/{managerId}")]
        public async Task<IActionResult> GetRoomsByManagerId(string managerId)
        {
            var (success, message, statusCode, data) = await _buildingService.GetRoomByManagerId(managerId);
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message,
                    data = data
                });
            }
            return StatusCode(statusCode, new
            {
                success = false,
                message = message
            });
        }
    }
}
