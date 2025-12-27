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

        // Get Students by Priority
        [HttpGet("priority")]
        public async Task<IActionResult> GetStudentsByPriority([FromQuery] string? priorityId)
        {
            var students = await _reportService.GetStudentsByPriorityAsync(priorityId);
            return Ok(new { success = true, data = students });
        }

        // Get Expired Contracts
        [HttpGet("expired-contracts")]
        public async Task<IActionResult> GetExpiredContracts([FromQuery] string? beforeDate)
        {
            DateOnly cutoff;
            if (string.IsNullOrWhiteSpace(beforeDate)) cutoff = DateOnly.FromDateTime(DateTime.UtcNow);
            else if (!DateOnly.TryParse(beforeDate, out cutoff)) return BadRequest(new { success = false, message = "Invalid date format (YYYY-MM-DD)" });

            var list = await _reportService.GetExpiredContractsAsync(cutoff);
            return Ok(new { success = true, data = list });
        }


        //Get Student Contracts
        [HttpGet("student/{studentId}/contracts")]
        public async Task<IActionResult> GetContractsByStudent(string studentId)
        {
            var list = await _reportService.GetContractsByStudentAsync(studentId);
            return Ok(new { success = true, data = list });
        }

        //Equipment Status For A Room
        [HttpGet("room/{roomId}/equipment")]
        public async Task<IActionResult> GetEquipmentByRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return BadRequest(new { success = false, message = "RoomId is required" });

            var items = await _reportService.GetEquipmentStatusByRoomAsync(roomId);
            return Ok(new { success = true, data = items });
        }

        //Get Building Managers
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagersReport()
        {
            var managers = await _buildingManagerService.GetAllManagersAsync();
            return Ok(new { success = true, data = managers });
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
                ws.Cell(1, 1).Value = "Ma phong";
                ws.Cell(1, 2).Value = "Ten phong";
                ws.Cell(1, 3).Value = "Loai phong";
                ws.Cell(1, 4).Value = "Suc chua";
                ws.Cell(1, 5).Value = "Dang o";
                ws.Cell(1, 6).Value = "Giuong trong";
                ws.Cell(1, 7).Value = "Gia";

                // header style
                var headerRange = ws.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#D9E1F2");
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

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

                ws.Range(1,1,r-1,7).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Range(1,1,r-1,7).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Available_Rooms.xlsx");
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
                ws.Cell(1, 1).Value = "Ma HD";
                ws.Cell(1, 2).Value = "Ma sinh vien";
                ws.Cell(1, 3).Value = "Ten sinh vien";
                ws.Cell(1, 4).Value = "Ma phong";
                ws.Cell(1, 5).Value = "Ten phong";
                ws.Cell(1, 6).Value = "Ngay ket thuc";
                ws.Cell(1, 7).Value = "Trang thai";

                var headerRange = ws.Range(1,1,1,7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FCE4D6");
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                int r = 2;
                foreach (var c in list)
                {
                    ws.Cell(r, 1).Value = c.ContractID;
                    ws.Cell(r, 2).Value = c.StudentID;
                    ws.Cell(r, 3).Value = c.StudentName;
                    ws.Cell(r, 4).Value = c.RoomID;
                    ws.Cell(r, 5).Value = c.RoomName;
                    ws.Cell(r, 6).Value = c.EndDate == DateOnly.MinValue ? string.Empty : c.EndDate.ToString("yyyy-MM-dd");
                    ws.Cell(r, 7).Value = c.ContractStatus;
                    r++;
                }

                ws.Range(1,1,r-1,7).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Range(1,1,r-1,7).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Expired_Contracts.xlsx");
        }

        // Export student contracts as Excel
        [HttpGet("export/student-contracts/{studentId}")]
        public async Task<IActionResult> ExportStudentContractsExcel(string studentId)
        {
            var list = await _reportService.GetContractsByStudentAsync(studentId);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("StudentContracts");
                ws.Cell(1, 1).Value = "Ma HD";
                ws.Cell(1, 2).Value = "Ma sinh vien";
                ws.Cell(1, 3).Value = "Ten sinh vien";
                ws.Cell(1, 4).Value = "Ma phong";
                ws.Cell(1, 5).Value = "Ten phong";
                ws.Cell(1, 6).Value = "Ngay bat dau";
                ws.Cell(1, 7).Value = "Ngay ket thuc";
                ws.Cell(1, 8).Value = "Trang thai";

                var headerRange = ws.Range(1,1,1,8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#D9EAD3");
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                int r = 2;
                foreach (var c in list)
                {
                    ws.Cell(r, 1).Value = c.ContractID;
                    ws.Cell(r, 2).Value = c.StudentID;
                    ws.Cell(r, 3).Value = c.StudentName;
                    ws.Cell(r, 4).Value = c.RoomID;
                    ws.Cell(r, 5).Value = c.RoomName;
                    ws.Cell(r, 6).Value = c.StartDate.ToString("yyyy-MM-dd");
                    ws.Cell(r, 7).Value = c.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                    ws.Cell(r, 8).Value = c.ContractStatus;
                    r++;
                }

                ws.Range(1,1,r-1,8).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Range(1,1,r-1,8).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Contracts_{studentId}.xlsx");
        }

        // Export priority students as Excel
        [HttpGet("export/priority-students")]
        public async Task<IActionResult> ExportPriorityStudentsExcel([FromQuery] string? priorityId)
        {
            var students = await _reportService.GetStudentsByPriorityAsync(priorityId);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("PriorityStudents");
                ws.Cell(1, 1).Value = "Ma sinh vien";
                ws.Cell(1, 2).Value = "Ho ten";
                ws.Cell(1, 3).Value = "Email";
                ws.Cell(1, 4).Value = "SDT";
                ws.Cell(1, 5).Value = "Ma uu tien";
                ws.Cell(1, 6).Value = "Ten uu tien";

                var headerRange = ws.Range(1,1,1,6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FFF2CC");
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                int r = 2;
                foreach (var s in students)
                {
                    ws.Cell(r, 1).Value = s.StudentID;
                    ws.Cell(r, 2).Value = s.FullName;
                    ws.Cell(r, 3).Value = s.Email;
                    ws.Cell(r, 4).Value = s.PhoneNumber;
                    ws.Cell(r, 5).Value = s.PriorityID;
                    ws.Cell(r, 6).Value = s.PriorityName;
                    r++;
                }

                ws.Range(1,1,r-1,6).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Range(1,1,r-1,6).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Priority_students.xlsx");
        }

        // Export room equipment as Excel
        [HttpGet("export/room-equipment/{roomId}")]
        public async Task<IActionResult> ExportRoomEquipmentExcel(string roomId)
        {
            var items = await _reportService.GetEquipmentStatusByRoomAsync(roomId);

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("RoomEquipment");
                ws.Cell(1, 1).Value = "Ma thiet bi";
                ws.Cell(1, 2).Value = "Ten thiet bi";
                ws.Cell(1, 3).Value = "So luong";
                ws.Cell(1, 4).Value = "Trang thai";
                ws.Cell(1, 5).Value = "Ma phong";
                ws.Cell(1, 6).Value = "Ten phong";

                var headerRange = ws.Range(1,1,1,6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#D1E7DD");
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                int r = 2;
                foreach (var e in items)
                {
                    ws.Cell(r, 1).Value = e.EquipmentID;
                    ws.Cell(r, 2).Value = e.EquipmentName;
                    ws.Cell(r, 3).Value = e.Quantity;
                    ws.Cell(r, 4).Value = e.Status;
                    ws.Cell(r, 5).Value = e.RoomID;
                    ws.Cell(r, 6).Value = e.RoomName;
                    r++;
                }

                ws.Range(1,1,r-1,6).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Range(1,1,r-1,6).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Room_{roomId}_equipment.xlsx");
        }

        // Export managers as Excel
        [HttpGet("export/managers")]
        public async Task<IActionResult> ExportManagersExcel()
        {
            var managers = await _buildingManagerService.GetAllManagersAsync();

            var bytes = _exportService.CreateExcel(wb =>
            {
                var ws = wb.Worksheets.Add("Managers");
                ws.Cell(1, 1).Value = "Ma quan ly";
                ws.Cell(1, 2).Value = "Ho ten";
                ws.Cell(1, 3).Value = "Email";
                ws.Cell(1, 4).Value = "SDT";
                ws.Cell(1, 5).Value = "Dia chi";
                ws.Cell(1, 6).Value = "So toa nha";
                ws.Cell(1, 7).Value = "Ten toa nha"; 

                var headerRange = ws.Range(1,1,1,7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#CFE2F3");
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                int r = 2;
                foreach (var m in managers)
                {
                    ws.Cell(r, 1).Value = m.ManagerID;
                    ws.Cell(r, 2).Value = m.FullName;
                    ws.Cell(r, 3).Value = m.Email;
                    ws.Cell(r, 4).Value = m.PhoneNumber;
                    ws.Cell(r, 5).Value = m.Address;
                    ws.Cell(r, 6).Value = m.Buildings?.Count() ?? 0;

                    var buildingNames = m.Buildings?.Select(b => b.BuildingName).ToArray() ?? Array.Empty<string>();
                    ws.Cell(r, 7).Value = string.Join(", ", buildingNames);

                    r++;
                }

                ws.Range(1,1,r-1,7).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Range(1,1,r-1,7).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
            });

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Managers.xlsx");
        }

        [HttpGet("admin-overview")]
        public async Task<IActionResult> GetAdminOverview()
        {
            try
            {
                var result = await _reportService.GetOverviewDashBoard();

                return Ok(new
                {
                    Success = true,
                    Message = "L?y d? li?u th?ng kê thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "?ã x?y ra l?i server: " + ex.Message
                });
            }
        }
    }
}
