using API.Services.Interfaces;
using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.DTOs.RoomDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using ClosedXML.Excel;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IBuildingManagerService _buildingManagerService;
        private readonly API.Services.Interfaces.IExportService _exportService;

        public ReportController(IReportService reportService, IBuildingManagerService buildingManagerService, IExportService exportService)
        {
            _reportService = reportService;
            _buildingManagerService = buildingManagerService;
            _exportService = exportService;
        }

        [HttpGet("priority")]
        public async Task<IActionResult> GetStudentsByPriority([FromQuery] string? priorityId)
        {
            var students = await _reportService.GetStudentsByPriorityAsync(priorityId);
            return Ok(new { success = true, data = students });
        }

        [HttpGet("expired-contracts")]
        public async Task<IActionResult> GetExpiredContracts([FromQuery] string? beforeDate)
        {
            DateOnly cutoff;
            if (string.IsNullOrWhiteSpace(beforeDate)) cutoff = DateOnly.FromDateTime(DateTime.UtcNow);
            else if (!DateOnly.TryParse(beforeDate, out cutoff)) return BadRequest(new { success = false, message = "Invalid date format (YYYY-MM-DD)" });

            var list = await _reportService.GetExpiredContractsAsync(cutoff);
            return Ok(new { success = true, data = list });
        }

        [HttpGet("student/{studentId}/contracts")]
        public async Task<IActionResult> GetContractsByStudent(string studentId)
        {
            var list = await _reportService.GetContractsByStudentAsync(studentId);
            return Ok(new { success = true, data = list });
        }

        // New endpoint: equipment status for a room
        [HttpGet("room/{roomId}/equipment")]
        public async Task<IActionResult> GetEquipmentByRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return BadRequest(new { success = false, message = "RoomId is required" });

            var items = await _reportService.GetEquipmentStatusByRoomAsync(roomId);
            return Ok(new { success = true, data = items });
        }

        // New report endpoint: building managers
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagersReport()
        {
            var managers = await _buildingManagerService.GetAllManagersAsync();
            return Ok(new { success = true, data = managers });
        }

        // Export managers as CSV
        [HttpGet("managers/export")]
        public async Task<IActionResult> ExportManagersCsv()
        {
            var managers = await _buildingManagerService.GetAllManagersAsync();
            var sb = new StringBuilder();
            sb.AppendLine("ManagerID,FullName,Email,PhoneNumber,Address,BuildingsCount");
            foreach (var m in managers)
            {
                var buildingsCount = m.Buildings?.Count() ?? 0;
                var line = $"\"{m.ManagerID}\",\"{m.FullName}\",\"{m.Email}\",\"{m.PhoneNumber}\",\"{m.Address}\",{buildingsCount}";
                sb.AppendLine(line);
            }

            var bytes = _exportService.CreateCsv(sb.ToString());
            return File(bytes, "text/csv", "managers_report.csv");
        }

        // Export expired contracts as CSV
        [HttpGet("expired-contracts/export")]
        public async Task<IActionResult> ExportExpiredContractsCsv([FromQuery] string? beforeDate)
        {
            DateOnly cutoff;
            if (string.IsNullOrWhiteSpace(beforeDate)) cutoff = DateOnly.FromDateTime(DateTime.UtcNow);
            else if (!DateOnly.TryParse(beforeDate, out cutoff)) return BadRequest(new { success = false, message = "Invalid date format (YYYY-MM-DD)" });

            var list = await _reportService.GetExpiredContractsAsync(cutoff);
            var sb = new StringBuilder();
            sb.AppendLine("ContractID,StudentID,StudentName,RoomID,EndDate,ContractStatus");
            foreach (var c in list)
            {
                var end = c.EndDate == DateOnly.MinValue ? string.Empty : c.EndDate.ToString("yyyy-MM-dd");
                var line = $"\"{c.ContractID}\",\"{c.StudentID}\",\"{c.StudentName}\",\"{c.RoomID}\",{end},\"{c.ContractStatus}\"";
                sb.AppendLine(line);
            }

            var bytes = _exportService.CreateCsv(sb.ToString());
            return File(bytes, "text/csv", "expired_contracts_report.csv");
        }

        // Export available rooms as Excel
        [HttpGet("export/available-rooms")]
        public async Task<IActionResult> ExportAvailableRoomsExcel([FromQuery] string? buildingId, [FromQuery] string? roomTypeId)
        {
            var filter = new BusinessObject.DTOs.RoomDTOs.RoomFilterDto { BuildingId = buildingId, RoomTypeId = roomTypeId, OnlyAvailable = true };
            var rooms = await _reportService.GetAvailableRoomsAsync(filter);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("AvailableRooms");
                ws.Cell(1, 1).Value = "RoomID";
                ws.Cell(1, 2).Value = "RoomName";
                ws.Cell(1, 3).Value = "RoomType";
                ws.Cell(1, 4).Value = "Capacity";
                ws.Cell(1, 5).Value = "Occupied";
                ws.Cell(1, 6).Value = "AvailableBeds";
                ws.Cell(1, 7).Value = "Price";

                int r = 2;
                foreach (var room in rooms)
                {
                    ws.Cell(r, 1).Value = room.RoomID;
                    ws.Cell(r, 2).Value = room.RoomName;
                    ws.Cell(r, 3).Value = room.RoomType;
                    ws.Cell(r, 4).Value = room.Capacity;
                    ws.Cell(r, 5).Value = room.Occupied;
                    ws.Cell(r, 6).Value = room.AvailableBeds;
                    ws.Cell(r, 7).Value = room.Price;
                    r++;
                }

                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "available_rooms.xlsx");
        }

        // Export expired contracts as Excel
        [HttpGet("export/expired-contracts")]
        public async Task<IActionResult> ExportExpiredContractsExcel([FromQuery] string? beforeDate)
        {
            DateOnly cutoff;
            if (string.IsNullOrWhiteSpace(beforeDate)) cutoff = DateOnly.FromDateTime(DateTime.UtcNow);
            else if (!DateOnly.TryParse(beforeDate, out cutoff)) return BadRequest(new { success = false, message = "Invalid date format (YYYY-MM-DD)" });

            var list = await _reportService.GetExpiredContractsAsync(cutoff);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("ExpiredContracts");
                ws.Cell(1, 1).Value = "ContractID";
                ws.Cell(1, 2).Value = "StudentID";
                ws.Cell(1, 3).Value = "StudentName";
                ws.Cell(1, 4).Value = "RoomID";
                ws.Cell(1, 5).Value = "EndDate";
                ws.Cell(1, 6).Value = "ContractStatus";

                int r = 2;
                foreach (var c in list)
                {
                    ws.Cell(r, 1).Value = c.ContractID;
                    ws.Cell(r, 2).Value = c.StudentID;
                    ws.Cell(r, 3).Value = c.StudentName;
                    ws.Cell(r, 4).Value = c.RoomID;
                    ws.Cell(r, 5).Value = c.EndDate == DateOnly.MinValue ? string.Empty : c.EndDate.ToString("yyyy-MM-dd");
                    ws.Cell(r, 6).Value = c.ContractStatus;
                    r++;
                }

                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "expired_contracts.xlsx");
        }

        // Export student contracts as Excel
        [HttpGet("export/student-contracts/{studentId}")]
        public async Task<IActionResult> ExportStudentContractsExcel(string studentId)
        {
            var list = await _reportService.GetContractsByStudentAsync(studentId);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("StudentContracts");
                ws.Cell(1, 1).Value = "ContractID";
                ws.Cell(1, 2).Value = "StudentID";
                ws.Cell(1, 3).Value = "StudentName";
                ws.Cell(1, 4).Value = "RoomID";
                ws.Cell(1, 5).Value = "StartDate";
                ws.Cell(1, 6).Value = "EndDate";
                ws.Cell(1, 7).Value = "ContractStatus";

                int r = 2;
                foreach (var c in list)
                {
                    ws.Cell(r, 1).Value = c.ContractID;
                    ws.Cell(r, 2).Value = c.StudentID;
                    ws.Cell(r, 3).Value = c.StudentName;
                    ws.Cell(r, 4).Value = c.RoomID;
                    ws.Cell(r, 5).Value = c.StartDate.ToString("yyyy-MM-dd");
                    ws.Cell(r, 6).Value = c.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                    ws.Cell(r, 7).Value = c.ContractStatus;
                    r++;
                }

                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"contracts_{studentId}.xlsx");
        }

        // Export priority students as Excel
        [HttpGet("export/priority-students")]
        public async Task<IActionResult> ExportPriorityStudentsExcel([FromQuery] string? priorityId)
        {
            var students = await _reportService.GetStudentsByPriorityAsync(priorityId);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("PriorityStudents");
                ws.Cell(1, 1).Value = "StudentID";
                ws.Cell(1, 2).Value = "FullName";
                ws.Cell(1, 3).Value = "Email";
                ws.Cell(1, 4).Value = "PhoneNumber";
                ws.Cell(1, 5).Value = "PriorityID";

                int r = 2;
                foreach (var s in students)
                {
                    ws.Cell(r, 1).Value = s.StudentID;
                    ws.Cell(r, 2).Value = s.FullName;
                    ws.Cell(r, 3).Value = s.Email;
                    ws.Cell(r, 4).Value = s.PhoneNumber;
                    ws.Cell(r, 5).Value = s.PriorityID;
                    r++;
                }

                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "priority_students.xlsx");
        }

        // Export room equipment as Excel
        [HttpGet("export/room-equipment/{roomId}")]
        public async Task<IActionResult> ExportRoomEquipmentExcel(string roomId)
        {
            var items = await _reportService.GetEquipmentStatusByRoomAsync(roomId);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("RoomEquipment");
                ws.Cell(1, 1).Value = "EquipmentID";
                ws.Cell(1, 2).Value = "EquipmentName";
                ws.Cell(1, 3).Value = "Status";
                ws.Cell(1, 4).Value = "RoomID";

                int r = 2;
                foreach (var e in items)
                {
                    ws.Cell(r, 1).Value = e.EquipmentID;
                    ws.Cell(r, 2).Value = e.EquipmentName;
                    ws.Cell(r, 3).Value = e.Status;
                    ws.Cell(r, 4).Value = e.RoomID;
                    r++;
                }

                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"room_{roomId}_equipment.xlsx");
        }

        // Export managers as Excel
        [HttpGet("export/managers")]
        public async Task<IActionResult> ExportManagersExcel()
        {
            var managers = await _buildingManagerService.GetAllManagersAsync();

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("Managers");
                ws.Cell(1, 1).Value = "ManagerID";
                ws.Cell(1, 2).Value = "FullName";
                ws.Cell(1, 3).Value = "Email";
                ws.Cell(1, 4).Value = "PhoneNumber";
                ws.Cell(1, 5).Value = "Address";
                ws.Cell(1, 6).Value = "BuildingsCount";

                int r = 2;
                foreach (var m in managers)
                {
                    ws.Cell(r, 1).Value = m.ManagerID;
                    ws.Cell(r, 2).Value = m.FullName;
                    ws.Cell(r, 3).Value = m.Email;
                    ws.Cell(r, 4).Value = m.PhoneNumber;
                    ws.Cell(r, 5).Value = m.Address;
                    ws.Cell(r, 6).Value = m.Buildings?.Count() ?? 0;
                    r++;
                }

                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "managers.xlsx");
        }
    }
}
