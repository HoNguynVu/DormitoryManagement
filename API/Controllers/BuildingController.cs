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
    }
}
