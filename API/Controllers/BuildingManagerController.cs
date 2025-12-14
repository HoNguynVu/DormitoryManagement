using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingManagerController : ControllerBase
    {
        private readonly IBuildingManagerService _service;

        public BuildingManagerController(IBuildingManagerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllManagersAsync();
            return Ok(new { success = true, data = list });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { success = false, message = "Manager id is required" });
            var m = await _service.GetManagerByIdAsync(id);
            if (m == null) return NotFound(new { success = false, message = "Manager not found" });
            return Ok(new { success = true, data = m });
        }
    }
}
