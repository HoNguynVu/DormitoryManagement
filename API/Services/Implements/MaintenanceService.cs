using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.MaintenanceDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IMaintenanceUow _uow;
        private readonly IRoomEquipmentService _roomEquipmentService;
        public MaintenanceService(IMaintenanceUow uow, IRoomEquipmentService roomEquipmentService)
        {
            _uow = uow;
            _roomEquipmentService = roomEquipmentService;
        }
        public async Task<(bool Success, string Message, int StatusCode,string? requestMaintenanceId)> CreateRequestAsync(CreateMaintenanceDto dto)
        {
            // Validation đầu vào
            if (dto == null || string.IsNullOrEmpty(dto.StudentId) || string.IsNullOrEmpty(dto.Description))
            {
                return (false, "Thông tin yêu cầu không hợp lệ (Thiếu StudentId hoặc Mô tả).", 400,null);
            }
            await _uow.BeginTransactionAsync();
            try
            {
                // Kiểm tra xem Sinh viên có đang ở KTX không (Phải có hợp đồng Active)
                var contract = await _uow.Contracts.GetActiveContractByStudentId(dto.StudentId);

                if (contract == null || contract.Room == null)
                {
                    return (false, "Sinh viên chưa có hợp đồng phòng hiệu lực, không thể gửi yêu cầu.", 403,null); ;
                }
                if (!string.IsNullOrEmpty(dto.EquipmentId))
                {
                    try
                    {
                        await _roomEquipmentService.ChangeStatusAsync(contract.RoomID,dto.EquipmentId,1,"Good","Under Maintenance");
                    }
                    catch (Exception ex)
                    {
                        return (false, ex.Message, 400,null);
                    }
                }

                // Tạo Entity mới
                var request = new MaintenanceRequest
                {
                    RequestID = "MT-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = dto.StudentId,
                    RoomID = contract.Room.RoomID,
                    Description = dto.Description,
                    EquipmentID = dto.EquipmentId,
                    Status = "Pending",            
                    RequestDate = DateTime.Now,
                    RepairCost = 0             
                };

                _uow.Maintenances.Add(request);
                await _uow.CommitAsync();

                return (true, "Gửi yêu cầu bảo trì thành công.", 201,request.RequestID);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryMaintenanceDto> dto)> GetRequestsByStudentIdAsync(string studentId)
        {   
            try
            {
                
                var requests = await _uow.Maintenances.GetMaintenanceByStudentIdAsync(studentId);
                if (requests == null || !requests.Any())
                {
                    return (true, "Không tìm thấy yêu cầu nào.", 200, Enumerable.Empty<SummaryMaintenanceDto>());
                }
                var result = requests.Select(m => new SummaryMaintenanceDto
                {
                    MaintenanceID = m.RequestID,
                    RoomName = m.Room.RoomName,
                    StudentName = m.Student.FullName,
                    EquipmentName = m.Equipment.EquipmentName,
                    Description = m.Description,
                    Status = m.Status,
                    IssueDate = DateOnly.FromDateTime(m.RequestDate),
                    ResolvedDate = m.ResolvedDate.HasValue ? DateOnly.FromDateTime(m.ResolvedDate.Value) : null,
                    RepairCost = m.RepairCost,
                }).ToList();
                return (true, "Lấy danh sách thành công.", 200, result);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi truy vấn: {ex.Message}", 500, Enumerable.Empty<SummaryMaintenanceDto>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateStatusAsync(UpdateMaintenanceStatusDto dto)
        {
            // 1. Validation
            if (string.IsNullOrEmpty(dto.RequestId) || string.IsNullOrEmpty(dto.NewStatus))
            {
                return (false, "Request ID and New Status are required.", 400);
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // 2. Tìm yêu cầu bảo trì
                var request = await _uow.Maintenances.GetMaintenanceByIdAsync(dto.RequestId);
                if (request == null)
                {
                    return (false, "Maintenance request not found.", 404);
                }

                // 3. Cập nhật thông tin chung
                request.Status = dto.NewStatus;
                request.ManagerNote = dto.ManagerNote;
                
                try
                {  
                    if (dto.NewStatus == "In Progress" && request.EquipmentID != null)
                    {
                        await _roomEquipmentService.ChangeStatusAsync(request.RoomID, request.EquipmentID, 1, "Under Maintenance", "Being Repaired");
                    }
                    
                    else if (dto.NewStatus == "Completed" && request.EquipmentID != null)
                    {
                        await _roomEquipmentService.ChangeStatusAsync(request.RoomID, request.EquipmentID, 1, "Being Repaired", "Good");
                    }
                }
                catch (Exception ex)
                {
                    return (false, ex.Message, 400);
                }
                // 4. Xử lý Logic khi trạng thái là "Completed"
                if (dto.NewStatus == "Completed")
                {
                    request.ResolvedDate = DateTime.Now;
                    request.RepairCost = dto.RepairCost;

                    // --- LOGIC SINH HÓA ĐƠN ---
                    if (dto.RepairCost > 0)
                    {
                        var receipt = new Receipt
                        {
                            ReceiptID = "RE-" + IdGenerator.GenerateUniqueSuffix(),
                            StudentID = request.StudentID,
                            Amount = dto.RepairCost,
                            PaymentType = PaymentConstants.TypeMaintenanceFee, // Đánh dấu loại thanh toán
                            Status = "Pending",             // Chờ thanh toán
                            Content = $"Phí sửa chữa cho yêu cầu {request.RequestID}: {request.Description}",
                            PrintTime = DateTime.Now
                        };

                        _uow.Receipts.Add(receipt);
                    }
                }

                // 5. Lưu xuống DB
                _uow.Maintenances.Update(request);
                await _uow.CommitAsync();

                string msg = (dto.NewStatus == "Completed" && dto.RepairCost > 0)
                    ? "Đã hoàn thành sửa chữa và tạo hóa đơn thu phí cho sinh viên."
                    : "Cập nhật trạng thái thành công.";

                return (true, msg, 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryMaintenanceDto> dto)> GetMaintenanceFiltered(string? keyword, string? status, string? equipmentName)
        {
            try
            {
                var requests = await _uow.Maintenances.GetMaintenanceFilteredAsync(keyword, status, equipmentName);
                if (requests == null || !requests.Any())
                {
                    return (true, "Không tìm thấy yêu cầu nào.", 200, Enumerable.Empty<SummaryMaintenanceDto>());
                }
                var result = requests.Select(m => new SummaryMaintenanceDto
                {
                    MaintenanceID = m.RequestID,
                    RoomName = m.Room.RoomName,
                    StudentName = m.Student.FullName,
                    EquipmentName = m.Equipment.EquipmentName,
                    Description = m.Description,
                    Status = m.Status,
                    IssueDate = DateOnly.FromDateTime(m.RequestDate),
                    ResolvedDate = m.ResolvedDate.HasValue ? DateOnly.FromDateTime(m.ResolvedDate.Value) : null,
                    RepairCost = m.RepairCost,
                    
                }).ToList();
                return (true, "Lấy danh sách thành công.", 200, result);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi truy vấn: {ex.Message}", 500, Enumerable.Empty<SummaryMaintenanceDto>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, DetailMaintenanceDto dto)> GetMaintenanceDetail(string maintenanceId)
        {
            try
            {
                var request = await _uow.Maintenances.GetMaintenanceDetailAsync(maintenanceId);
                if (request == null)
                {
                    return (false, "Yêu cầu bảo trì không tồn tại.", 404, null!);
                }
                var dto = new DetailMaintenanceDto
                {
                    MaintenanceID = request.RequestID,
                    StudentName = request.Student.FullName,
                    RoomName = request.Room.RoomName,
                    EquipmentName = request.Equipment?.EquipmentName ?? "N/A",
                    Description = request.Description,
                    Status = request.Status,
                    IssueDate = DateOnly.FromDateTime(request.RequestDate),
                    ResolvedDate = request.ResolvedDate.HasValue ? DateOnly.FromDateTime(request.ResolvedDate.Value) : null,
                    ManagerNote = request.ManagerNote,
                    RepairCost = request.RepairCost,
                };
                return (true, "Lấy chi tiết thành công.", 200, dto);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi truy vấn: {ex.Message}", 500, null!);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, Dictionary<string, int> list)> GetOverviewMaintenance()
        {
            try
            {
                var maintenances = await _uow.Maintenances.GetAllAsync();
                var overview = maintenances
                    .GroupBy(m => m.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                return (true, "Lấy tổng quan bảo trì thành công.", 200, overview);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi truy vấn: {ex.Message}", 500, new Dictionary<string, int>());
            }
        }
    }
}
