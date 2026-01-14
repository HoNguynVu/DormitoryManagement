using API.Hubs;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.ViolationDTOs;
using BusinessObject.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;

namespace API.Services.Implements
{
    public class ViolationService : IViolationService
    {
        private readonly IViolationUow _violationUow;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private const int MAX_VIOLATIONS_BEFORE_TERMINATION = 3;

        public ViolationService(IViolationUow violationUow, IHubContext<NotificationHub> hubContext, IEmailService emailService)
        {
            _violationUow = violationUow;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message, int StatusCode, ViolationResponse? Data)> CreateViolationAsync(CreateViolationRequest request)
        {
            try
            {
                // ===== TRANSACTION 1: CREATE VIOLATION =====
                await _violationUow.BeginTransactionAsync();
                var manager = await _violationUow.BuildingManagers.GetByAccountIdAsync(request.AccountId);
                if (manager == null)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "Building manager not found.", 404, null);
                }
                var newViolation = new Violation
                {
                    ViolationID = "VL-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = request.StudentId,
                    ReportingManagerID = manager.ManagerID,
                    ViolationAct = request.ViolationAct,
                    Description = request.Description,
                    ViolationTime = DateTime.Now,
                    Resolution = "Đang chờ"
                };

                var account = await _violationUow.Accounts.GetAccountByStudentId(request.StudentId);
                if (account == null) {
                    await _violationUow.RollbackAsync();
                    return (false, "Account not found for the given student ID.", 404, null);
                }

                var newNoti = NotificationServiceHelpers.CreateNew(
                    accountId: account.UserId,
                    title: "Bạn đã có vi phạm mới!",
                    message: $"Vi phạm: '{newViolation.ViolationAct}' vừa được ghi nhận. Vui lòng kiểm tra và liên hệ ban quản lý để giái quyết.",
                    type: "Violation"
                );

                _violationUow.Notifications.Add(newNoti);
                _violationUow.Violations.Add(newViolation);
                await _violationUow.CommitAsync(); // Commit violation
                
                // ===== QUERY: COUNT VIOLATIONS (no transaction needed) =====
                var totalViolations = await _violationUow.Violations.CountViolationsByStudentId(request.StudentId);
                
                // ===== TRANSACTION 2: TERMINATE CONTRACT (if needed) =====
                if (totalViolations >= MAX_VIOLATIONS_BEFORE_TERMINATION)
                {
                    await _violationUow.BeginTransactionAsync(); // New transaction
                    try
                    {
                        var activeContract = await _violationUow.Contracts.GetActiveContractByStudentId(request.StudentId);
                        if (activeContract != null)
                        {
                            activeContract.ContractStatus = "Terminated";
                            activeContract.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                            _violationUow.Contracts.Update(activeContract);
                        }
                        var notiTermination = NotificationServiceHelpers.CreateNew(
                            accountId: account.UserId,
                            title: "Hợp đồng của bạn đã bị chấm dứt!",
                            message: "Do bạn đã vi phạm nội quy 3 lần, hợp đồng của bạn đã bị chấm dứt. Vui lòng liên hệ quản lý tòa nhà để biết thêm chi tiết.",
                            type: "Contract"
                            );
                        _violationUow.Notifications.Add(notiTermination);
                        await _violationUow.CommitAsync(); // Commit contract update

                        try
                        {
                            var dto = new DormTerminationDto
                            {
                                ContractCode = activeContract.ContractID,
                                StudentName = activeContract.Student.FullName,
                                StudentEmail = activeContract.Student.Email,
                                StudentId = activeContract.StudentID,
                                BuildingName = activeContract.Room.Building.BuildingName,
                                RoomName = activeContract.Room.RoomName,
                                TerminationDate = DateOnly.FromDateTime(DateTime.Now)
                            };
                            await _emailService.SendTerminatedNotiToStudentAsync(dto);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Email Error: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _violationUow.RollbackAsync();
                        return (false, $"Error terminating contract: {ex.Message}", 500, null);
                    }
                }

                // ===== RETURN RESPONSE =====
                var createdViolation = await _violationUow.Violations.GetByIdAsync(newViolation.ViolationID);
                if (createdViolation == null)
                {
                    return (false, "Failed to retrieve created violation.", 500, null);
                }

                var response = MapToResponse(createdViolation, totalViolations);

                string message = totalViolations >= MAX_VIOLATIONS_BEFORE_TERMINATION
                    ? "Violation created. Student has 3 violations, contract terminated."
                    : "Violation created successfully.";

                return (true, message, 201, response);
            }
            catch (Exception ex)
            {
                await _violationUow.RollbackAsync();
                return (false, $"Error creating violation: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateViolationAsync(UpdateViolationRequest request)
        {
            await _violationUow.BeginTransactionAsync();
            try
            {
                var violation = await _violationUow.Violations.GetByIdAsync(request.ViolationId);
                if (violation == null)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "Violation not found.", 404);
                }
                var account = await _violationUow.Accounts.GetAccountByStudentId(violation.StudentID);

                if (account == null)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "Account not found.", 404);
                }

                var newNoti = NotificationServiceHelpers.CreateNew(
                    accountId: account.UserId,
                    title: "Vi phạm đã bị xử lý!",
                    message: $"Vi phạm: '{violation.ViolationAct}' của bạn đã được xử lí như sau: {request.Resolution}",
                    type: "Violation"
                );

                violation.Resolution = request.Resolution;
                _violationUow.Violations.Update(violation);
                _violationUow.Notifications.Add(newNoti);

                await _violationUow.CommitAsync();
                try
                {
                    await _hubContext.Clients.User(newNoti.AccountID).SendAsync("ReceiveNotification", newNoti);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Error: {ex.Message}");
                }

            }
            catch (Exception ex)
            {
                await _violationUow.RollbackAsync();
                return (false, $"Error updating resolution: {ex.Message}", 500);
            }
            
            return (true, "Resolution updated successfully.", 200);
        }

        public async Task<(bool Success, string Message, int StatusCode)> DeleteViolationAsync(string violationId, string managerAccountId)
        {
            await _violationUow.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra quyền của manager
                var manager = await _violationUow.BuildingManagers.GetByAccountIdAsync(managerAccountId);
                if (manager == null)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "Building manager not found.", 404);
                }

                // 2. Lấy thông tin vi phạm
                var violation = await _violationUow.Violations.GetByIdAsync(violationId);
                if (violation == null)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "Violation not found.", 404);
                }

                // 3. Kiểm tra vi phạm có thuộc quản lý của manager không
                if (violation.ReportingManagerID != manager.ManagerID)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "You can only delete violations reported by you.", 403);
                }

                var studentId = violation.StudentID;
                
                // 4. Đếm số vi phạm hiện tại của sinh viên (trước khi xóa)
                var currentViolationCount = await _violationUow.Violations.CountViolationsByStudentId(studentId);

                // 5. Xóa vi phạm
                _violationUow.Violations.Delete(violation);
                
                // 6. Lấy thông tin account của sinh viên
                var account = await _violationUow.Accounts.GetAccountByStudentId(studentId);
                if (account == null)
                {
                    await _violationUow.RollbackAsync();
                    return (false, "Student account not found.", 404);
                }

                // 7. Tạo thông báo cho sinh viên về việc xóa vi phạm
                var deleteNoti = NotificationServiceHelpers.CreateNew(
                    accountId: account.UserId,
                    title: "Vi phạm đã được xóa!",
                    message: $"Vi phạm: '{violation.ViolationAct}' đã được trưởng tòa xóa. Đây là cơ hội để bạn cải thiện.",
                    type: "Violation"
                );
                _violationUow.Notifications.Add(deleteNoti);

                // 8. Kiểm tra nếu sinh viên đã bị terminate và sau khi xóa sẽ xuống dưới 3 vi phạm
                bool shouldRestoreContract = currentViolationCount >= MAX_VIOLATIONS_BEFORE_TERMINATION;
                
                if (shouldRestoreContract)
                {
                    // Số vi phạm sau khi xóa
                    var newViolationCount = currentViolationCount - 1;
                    
                    if (newViolationCount < MAX_VIOLATIONS_BEFORE_TERMINATION)
                    {
                        // Tìm hợp đồng bị terminate gần nhất
                        var terminatedContract = await _violationUow.Contracts
                            .FirstOrDefaultAsync(c => c.StudentID == studentId && c.ContractStatus == "Terminated");
                        
                        if (terminatedContract != null)
                        {
                            // Khôi phục hợp đồng
                            terminatedContract.ContractStatus = "Active";
                            _violationUow.Contracts.Update(terminatedContract);

                            // Thông báo khôi phục hợp đồng
                            var restoreNoti = NotificationServiceHelpers.CreateNew(
                                accountId: account.UserId,
                                title: "Hợp đồng đã được khôi phục!",
                                message: $"Do số vi phạm của bạn đã giảm xuống dưới {MAX_VIOLATIONS_BEFORE_TERMINATION}, hợp đồng của bạn đã được khôi phục. Hãy tuân thủ nội quy để tránh vi phạm trong tương lai.",
                                type: "Contract"
                            );
                            _violationUow.Notifications.Add(restoreNoti);
                        }
                    }
                }

                await _violationUow.CommitAsync();

                // 9. Gửi thông báo qua SignalR
                try
                {
                    await _hubContext.Clients.User(account.UserId).SendAsync("ReceiveNotification", deleteNoti);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Error: {ex.Message}");
                }

                string message = shouldRestoreContract && (currentViolationCount - 1) < MAX_VIOLATIONS_BEFORE_TERMINATION
                    ? "Violation deleted successfully. Student's contract has been restored."
                    : "Violation deleted successfully.";

                return (true, message, 200);
            }
            catch (Exception ex)
            {
                await _violationUow.RollbackAsync();
                return (false, $"Error deleting violation: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetViolationsByStudentIdAsync(string studentId)
        {
            try
            {
                var violations = await _violationUow.Violations.GetViolationsByStudentId(studentId);
                var totalCount = violations.Count();

                var response = violations.Select(v => MapToResponse(v, totalCount));

                return (true, "Violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetViolationsByStudentAccountIdAsync(string accountId)
        {
            try
            {
                if(string.IsNullOrEmpty(accountId))
                {
                    return (false, "Account ID is required.", 400, null);
                }
                var student = await _violationUow.Students.GetStudentByAccountIdAsync(accountId);
                if (student == null)
                {
                    return (false, "Student not found for the given account ID.", 404, null);
                }
                var violations = await _violationUow.Violations.GetViolationsByStudentId(student.StudentID);
                var totalCount = violations.Count();
                var response = violations.Select(v => MapToResponse(v, totalCount));
                return (true, "Violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetAllViolationsAsync()
        {
            try
            {
                var violations = await _violationUow.Violations.GetAllAsync();
                
                var response = violations.Select(v => 
                {
                    var count = violations.Count(x => x.StudentID == v.StudentID);
                    return MapToResponse(v, count);
                });

                return (true, "All violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetAllViolationsByManagerAsync(string accountId)
        {
            try
            {
                var manager = await _violationUow.BuildingManagers.GetByAccountIdAsync(accountId);
                if (manager == null)
                    return (false, "Building manager not found", 404, Enumerable.Empty<ViolationResponse>());

                var vioList = await _violationUow.Violations.GetByManagerId(manager.ManagerID);

                if (!vioList.Any())
                    return (true, "No violations found.", 200, Enumerable.Empty<ViolationResponse>());

                var violationCounts = vioList
                    .GroupBy(v => v.StudentID)
                    .ToDictionary(g => g.Key, g => g.Count());

                var dtoList = new List<ViolationResponse>(vioList.Count());

                foreach (var vio in vioList)
                {
                    var contract = await _violationUow.Contracts.GetActiveContractByStudentId(vio.StudentID);

                    var dto = new ViolationResponse
                    {
                        StudentId = vio.StudentID,
                        StudentName = vio.Student?.FullName ?? "N/A",
                        ReportingManagerId = vio.ReportingManagerID,

                        RoomId = contract?.RoomID ?? "N/A",
                        RoomName = contract?.Room?.RoomName ?? "N/A",

                        ViolationId = vio.ViolationID,
                        ViolationAct = vio.ViolationAct,
                        Description = vio.Description,
                        Resolution = vio.Resolution,
                        ViolationTime = vio.ViolationTime,

                        TotalViolationsOfStudent = violationCounts.ContainsKey(vio.StudentID)
                                                   ? violationCounts[vio.StudentID]
                                                   : 0
                    };
                    dtoList.Add(dto);
                }

                return (true, "Violations retrieved successfully.", 200, dtoList);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetPendingViolationsAsync()
        {
            try
            {
                var violations = await _violationUow.Violations.GetPendingViolations();
                
                var response = violations.Select(v => 
                {
                    var count = violations.Count(x => x.StudentID == v.StudentID);
                    return MapToResponse(v, count);
                });

                return (true, "Pending violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving pending violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationStats>? Data)> GetViolationStatsByManagerAsync(string accountId)
        {
            // 1. Lấy thông tin Manager
            var manager = await _violationUow.BuildingManagers.GetByAccountIdAsync(accountId);
            if (manager == null)
                return (false, "Building manager not found", 404, Enumerable.Empty<ViolationStats>());

            var violations = await _violationUow.Violations.GetByManagerId(manager.ManagerID);

            if (!violations.Any())
                return (true, "No violations found", 200, Enumerable.Empty<ViolationStats>());

            var statsList = new List<ViolationStats>();

            var studentGroups = violations.GroupBy(v => v.StudentID);

            foreach (var group in studentGroups)
            {
                var studentId = group.Key;
                var firstViolation = group.First(); 
                var totalCount = group.Count(); 

                var contract = await _violationUow.Contracts.GetActiveContractByStudentId(studentId);

                var dto = new ViolationStats
                {
                    StudentId = studentId,
                    StudentName = firstViolation.Student?.FullName ?? "Unknown",

                    RoomId = contract?.RoomID ?? "N/A",
                    RoomName = contract?.Room?.RoomName ?? "N/A", 
                    TotalViolations = totalCount
                };

                statsList.Add(dto);
            }

            return (true, "Successfully", 200, statsList);
        }

        private ViolationResponse MapToResponse(Violation violation, int totalViolations)
        {
            return new ViolationResponse
            {
                ViolationId = violation.ViolationID,
                StudentId = violation.StudentID,
                StudentName = violation.Student?.FullName ?? "Unknown",
                ReportingManagerId = violation.ReportingManagerID,
                ReportingManagerName = violation.ReportingManager?.FullName,
                RoomId = violation.Student != null && violation.Student.Contracts != null && violation.Student.Contracts.Any(c => c.ContractStatus == "Active")
                            ? violation.Student.Contracts.First(c => c.ContractStatus == "Active").RoomID
                            : null,
                RoomName = violation.Student != null && violation.Student.Contracts != null && violation.Student.Contracts.Any(c => c.ContractStatus == "Active")
                            ? violation.Student.Contracts.First(c => c.ContractStatus == "Active").Room.RoomName : null,
                ViolationAct = violation.ViolationAct,
                ViolationTime = violation.ViolationTime,
                Description = violation.Description,
                Resolution = violation.Resolution,
                TotalViolationsOfStudent = totalViolations
            };
        }
    }
}
